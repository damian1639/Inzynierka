using System.Collections.Generic;
using UnityEngine;

// UWAGA: już NIE implementujemy tutaj IConveyorConsumer – wejściami zajmą się osobne skrypty
[RequireComponent(typeof(Collider))]
public class Assembler : MonoBehaviour
{
    [System.Serializable]
    public class CombineRecipe
    {
        public ItemDefinition inputA;   // np. CopperPlate
        public ItemDefinition inputB;   // np. Brick
        public ItemDefinition output;   // np. ReinforcedPlate
        public float craftTime = 10f;   // czas tworzenia
        public int amountA = 1;         // ile sztuk A
        public int amountB = 1;         // ile sztuk B
        public int outputAmount = 1;    // ile sztuk na wyjściu
    }

    [Header("Recepty (edytuj w Inspectorze)")]
    public List<CombineRecipe> recipes = new List<CombineRecipe>();

    [Header("Stan wejść/wyjścia")]
    public string currentAId = null;
    public string currentBId = null;
    public int countA = 0;
    public int countB = 0;

    public string outputId = null;
    public int outputCount = 0;

    [Header("Postęp")]
    public bool isWorking = false;
    public float progress01 = 0f;

    private float _timer = 0f;
    private CombineRecipe _activeRecipe = null;

    [Header("Taśmociągi – wyjście")]
    public Transform outputPoint;      // gdzie wychodzi gotowy item
    public LayerMask consumerMask;     // np. Buildable
    public float pushInterval = 0.2f;
    public float outputRadius = 0.25f;

    private float _pushTimer = 0f;
    private IConveyorConsumer _cachedConsumer;

    private void Update()
    {
        // ----- KRAFTOWANIE -----
        if (isWorking && _activeRecipe != null)
        {
            _timer += Time.deltaTime;
            progress01 = Mathf.Clamp01(_timer / _activeRecipe.craftTime);

            if (_timer >= _activeRecipe.craftTime)
            {
                _timer = 0f;
                progress01 = 0f;

                // zużyj wejścia
                countA -= _activeRecipe.amountA;
                countB -= _activeRecipe.amountB;
                if (countA < 0) countA = 0;
                if (countB < 0) countB = 0;

                // dodaj wynik
                if (!string.IsNullOrEmpty(outputId))
                {
                    outputCount += _activeRecipe.outputAmount;
                }

                // czy możemy robić dalej z pozostałych materiałów?
                if (!HasEnoughForRecipe(_activeRecipe))
                {
                    isWorking = false;
                    _activeRecipe = null;
                }
                else
                {
                    StartCraftingInternal();
                }
            }
        }

        // ----- WYJŚCIE NA TAŚMĘ -----
        TryPushOutputToConsumer();
    }

    // ================== API dla UI (z ekwipunku) ==================

    public bool InsertInputA(Inventory playerInv, string resourceId, int amount = 1)
    {
        if (amount <= 0 || playerInv == null) return false;

        int have = playerInv.GetResourceCount(resourceId);
        if (have <= 0) return false;

        if (string.IsNullOrEmpty(currentAId))
            currentAId = null;

        if (!string.IsNullOrEmpty(currentAId) && currentAId != resourceId)
            return false;

        if (!playerInv.RemoveResource(resourceId, amount))
            return false;

        if (string.IsNullOrEmpty(currentAId))
            currentAId = resourceId;

        countA += amount;

        TrySetupActiveRecipe();
        return true;
    }

    public bool InsertInputB(Inventory playerInv, string resourceId, int amount = 1)
    {
        if (amount <= 0 || playerInv == null) return false;

        int have = playerInv.GetResourceCount(resourceId);
        if (have <= 0) return false;

        if (string.IsNullOrEmpty(currentBId))
            currentBId = null;

        if (!string.IsNullOrEmpty(currentBId) && currentBId != resourceId)
            return false;

        if (!playerInv.RemoveResource(resourceId, amount))
            return false;

        if (string.IsNullOrEmpty(currentBId))
            currentBId = resourceId;

        countB += amount;

        TrySetupActiveRecipe();
        return true;
    }

    public int TakeAllOutputToInventory(Inventory playerInv)
    {
        if (playerInv == null || outputCount <= 0 || string.IsNullOrEmpty(outputId))
            return 0;

        playerInv.AddResource(outputId, outputCount);
        int taken = outputCount;
        outputCount = 0;
        return taken;
    }
public void StartCrafting()
{
    // Start tylko gdy NIE trwa już craft
    if (_activeRecipe != null && HasEnoughForRecipe(_activeRecipe) && !isWorking)
    {
        StartCraftingInternal();
    }
}


    private void StartCraftingInternal()
    {
        isWorking = true;
        _timer = 0f;
        progress01 = 0f;
    }

    // ================== WEJŚCIA Z TAŚM – PUBLICZNE DLA INPUT A/B ==================

    // wywoływane przez InputAConsumer
    public bool AcceptFromBeltToSlotA(string resourceId)
    {
        return AcceptFromBeltToSlot(resourceId, true);
    }

    // wywoływane przez InputBConsumer
    public bool AcceptFromBeltToSlotB(string resourceId)
    {
        return AcceptFromBeltToSlot(resourceId, false);
    }

    private bool AcceptFromBeltToSlot(string resourceId, bool toSlotA)
    {
        if (string.IsNullOrEmpty(resourceId))
            return false;

        // traktujemy "" jak null
        if (string.IsNullOrEmpty(currentAId)) currentAId = null;
        if (string.IsNullOrEmpty(currentBId)) currentBId = null;

        if (!CanAcceptToSlot(resourceId, toSlotA))
            return false;

        if (toSlotA)
        {
            if (currentAId == null) currentAId = resourceId;
            countA += 1;
        }
        else
        {
            if (currentBId == null) currentBId = resourceId;
            countB += 1;
        }

        TrySetupActiveRecipe();
        return true;
    }

    private bool CanAcceptToSlot(string resourceId, bool slotA)
    {
        string cur = slotA ? currentAId : currentBId;

        // jeśli slot ma już typ i to nie to samo – nie przyjmujemy
        if (!string.IsNullOrEmpty(cur) && cur != resourceId)
            return false;

        // czy ten item w ogóle występuje w jakiejś recepcie (jako input)
        foreach (var r in recipes)
        {
            if (r == null || r.inputA == null || r.inputB == null) continue;
            if (r.inputA.id == resourceId || r.inputB.id == resourceId)
                return true;
        }
        return false;
    }

    // ================== WYJŚCIE NA TAŚMĘ ==================

    private void TryPushOutputToConsumer()
    {
        if (outputCount <= 0 || string.IsNullOrEmpty(outputId))
            return;

        _pushTimer += Time.deltaTime;
        if (_pushTimer < pushInterval) return;
        _pushTimer = 0f;

        if (_cachedConsumer == null)
            CacheConsumer();

        if (_cachedConsumer == null) return;

        if (_cachedConsumer.TryAcceptItem(outputId))
        {
            outputCount--;
        }
    }

    private void CacheConsumer()
    {
        _cachedConsumer = null;
        if (outputPoint == null) return;

        Vector3 pos = outputPoint.position;
        Collider[] hits = Physics.OverlapSphere(
            pos,
            outputRadius,
            consumerMask,
            QueryTriggerInteraction.Ignore);

        foreach (var h in hits)
        {
            var c = h.GetComponentInParent<IConveyorConsumer>();
            if (c != null)
            {
                _cachedConsumer = c;
                break;
            }
        }
    }

    // ================== LOGIKA PRZEPISÓW ==================

private void TrySetupActiveRecipe()
{
    _activeRecipe = null;
    outputId = null;

    if (string.IsNullOrEmpty(currentAId) || string.IsNullOrEmpty(currentBId))
        return;

    foreach (var r in recipes)
    {
        if (r == null || r.inputA == null || r.inputB == null || r.output == null)
            continue;

        bool matchAB = r.inputA.id == currentAId && r.inputB.id == currentBId;
        bool matchBA = r.inputA.id == currentBId && r.inputB.id == currentAId;

        if (matchAB || matchBA)
        {
            _activeRecipe = r;
            outputId = r.output.id;

            // UWAGA: NIE restartujemy crafta, jeśli już trwa.
            if (HasEnoughForRecipe(_activeRecipe) && !isWorking)
                StartCraftingInternal();

            return;
        }
    }

    // brak pasującej recepty – wyczyść stan
    _activeRecipe = null;
    outputId = null;
    isWorking = false;
    progress01 = 0f;
    _timer = 0f;
}


    private bool HasEnoughForRecipe(CombineRecipe r)
    {
        if (r == null) return false;
        return countA >= r.amountA && countB >= r.amountB;
    }
}
