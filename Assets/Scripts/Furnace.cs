using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Furnace : MonoBehaviour, IConveyorConsumer
{
    [System.Serializable]
    public class SmeltingRecipe
    {
        public ItemDefinition input;   // np. Stone
        public ItemDefinition output;  // np. Brick
    }

    [Header("Czasy")]
    public float processTime = 2f;

    [Header("Recepty (edytuj w Inspectorze)")]
    public List<SmeltingRecipe> recipes = new List<SmeltingRecipe>();

    [Header("Stan")]
    public string currentInput = null;   // id surowca (spójne z ItemDefinition.id / Inventory)
    public string currentOutput = null;
    public int inputCount = 0;
    public int outputCount = 0;
    public bool isWorking = false;
    public float progress01 = 0f;

    private float _timer = 0f;

    [Header("Wejście z taśmy")]
    public Transform intakePoint;  // dziecko z przodu pieca, gdzie dochodzi taśma

    [Header("Wyjście na taśmę")]
    public Transform outputPoint;          // dziecko z tyłu/boku pieca
    public LayerMask consumerMask;         // maska dla taśmociągów / skrzyń (np. Buildable)
    public float pushInterval = 0.2f;      // co ile sekund próbujemy wypchnąć item
    public float outputRadius = 0.25f;     // promień szukania konsumenta przy outputPoint

    private float _pushTimer = 0f;
    private IConveyorConsumer _cachedConsumer;

    private void Awake()
    {
        // czysty start
        currentInput = null;
        currentOutput = null;
        inputCount = 0;
        outputCount = 0;
        isWorking = false;
        progress01 = 0f;
        _timer = 0f;
    }

    private void Update()
    {
        // SMELTOWANIE
        if (isWorking)
        {
            _timer += Time.deltaTime;
            progress01 = Mathf.Clamp01(_timer / processTime);

            if (_timer >= processTime)
            {
                _timer = 0f;
                progress01 = 0f;
                inputCount--;
                outputCount++;

                if (inputCount <= 0)
                    isWorking = false;
            }
        }

        // PRÓBA WYPCHNIĘCIA GOTOWYCH ITEMÓW NA TAŚMĘ
        TryPushOutputToConsumer();
    }

    // --- pomocnicze: szukanie recepty po id wejścia ---
    private bool TryGetRecipeByInputId(string inputId, out string outputId)
    {
        foreach (var r in recipes)
        {
            if (r != null && r.input != null && r.output != null && r.input.id == inputId)
            {
                outputId = r.output.id;
                return true;
            }
        }
        outputId = null;
        return false;
    }

    public bool CanAccept(string resourceId)
    {
        return currentInput == null || currentInput == resourceId;
    }

    // --- używane przez UI: wkładanie z ekwipunku ---
    public bool InsertFromInventory(Inventory inv, string resourceId, int amount = 1)
    {
        if (amount <= 0) return false;
        if (!CanAccept(resourceId)) return false;

        if (!TryGetRecipeByInputId(resourceId, out var outId)) return false;

        if (inv.RemoveResource(resourceId, amount))
        {
            if (currentInput == null)
            {
                currentInput = resourceId;
                currentOutput = outId;
            }

            inputCount += amount;

            if (!isWorking)
            {
                isWorking = true;
                _timer = 0f;
                progress01 = 0f;
            }
            return true;
        }
        return false;
    }

    public int TakeAllOutputToInventory(Inventory inv)
    {
        if (outputCount <= 0) return 0;

        inv.AddResource(currentOutput, outputCount);
        int taken = outputCount;
        outputCount = 0;
        return taken;
    }

    public void TryResetTypeIfEmpty()
    {
        if (inputCount <= 0 && outputCount <= 0)
        {
            currentInput = null;
            currentOutput = null;
            isWorking = false;
            progress01 = 0f;
            _timer = 0f;
        }
    }

    // --- wejście z taśmy / zewnętrznego źródła ---
    public bool AcceptFromBelt(string resourceId, int amount = 1)
    {
        if (amount <= 0) return false;

        // jeśli piec nie ma jeszcze ustawionego typu wejścia – sprawdź, czy jest recepta
        if (currentInput == null)
        {
            if (!TryGetRecipeByInputId(resourceId, out var outId))
                return false;  // brak recepty na ten surowiec

            currentInput = resourceId;
            currentOutput = outId;
        }
        else
        {
            // jeśli już coś przepala, typ musi się zgadzać
            if (currentInput != resourceId)
                return false;
        }

        inputCount += amount;

        if (!isWorking)
        {
            isWorking = true;
            _timer = 0f;
            progress01 = 0f;
        }

        return true;
    }

    // --- implementacja IConveyorConsumer (WEJŚCIE) ---
    public bool TryAcceptItem(string resourceId)
    {
        // taśma „woła” to dla każdej sztuki
        return AcceptFromBelt(resourceId, 1);
    }

    public Vector3 GetIntakePosition()
    {
        if (intakePoint != null)
            return intakePoint.position;

        // rezerwowy: punkt przed piecem
        return transform.position + transform.forward * 0.5f;
    }

    // --- WYJŚCIE: wypychanie gotowych itemów do IConveyorConsumer (np. belt/szyna) ---

    private void TryPushOutputToConsumer()
    {
        if (outputCount <= 0 || string.IsNullOrEmpty(currentOutput))
            return;

        _pushTimer += Time.deltaTime;
        if (_pushTimer < pushInterval) return;
        _pushTimer = 0f;

        if (_cachedConsumer == null)
            CacheConsumer();

        if (_cachedConsumer == null) return;

        // spróbuj wcisnąć 1 sztukę do konsumenta (np. taśma, skrzynia, inny blok)
        if (_cachedConsumer.TryAcceptItem(currentOutput))
        {
            outputCount--;
            TryResetTypeIfEmpty();
        }
    }

    private void CacheConsumer()
    {
        _cachedConsumer = null;
        if (outputPoint == null) return;

        Vector3 pos = outputPoint.position;
        Collider[] hits = Physics.OverlapSphere(pos, outputRadius, consumerMask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            var consumer = h.GetComponentInParent<IConveyorConsumer>();
            if (consumer != null && consumer != (IConveyorConsumer)this)
            {
                _cachedConsumer = consumer;
                break;
            }
        }
    }
}
