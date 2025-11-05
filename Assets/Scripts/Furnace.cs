using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Furnace : MonoBehaviour
{
    [Header("Czasy")]
    public float processTime = 2f; // sekundy/sztukę

    [Header("Stan")]
    public string currentInput = null; // "Stone" albo "Copper"
    public string currentOutput = null; // "Brick" albo "CopperPlate"
    public int inputCount = 0;   // ile sztuk czeka w piecu
    public int outputCount = 0;  // ile gotowych w piecu
    public bool isWorking = false;
    public float progress01 = 0f; // 0-1 postęp aktualnego elementu

    private Dictionary<string, string> recipes = new Dictionary<string, string>()
    {
        { "Stone",  "Brick" },
        { "Copper", "CopperPlate" }
    };

    private float _timer = 0f;

    private void Awake()
    {
    // start od „pustego” i gotowego stanu
    currentInput = null;
    currentOutput = null;
    inputCount = 0;
    outputCount = 0;
    isWorking = false;
    progress01 = 0f;
    }

    private void Update()
    {
        if (isWorking)
        {
            _timer += Time.deltaTime;
            progress01 = Mathf.Clamp01(_timer / processTime);

            if (_timer >= processTime)
            {
                // skończyliśmy 1 sztukę
                _timer = 0f;
                progress01 = 0f;
                inputCount--;
                outputCount++;

                if (inputCount <= 0)
                {
                    isWorking = false; // brak kolejnych sztuk
                }
            }
        }
    }

    public bool CanAccept(string resourceName)
    {
        // Możemy przyjąć tylko pierwszy wsad lub taki sam jak aktualny
        return currentInput == null || currentInput == resourceName;
    }

    public bool InsertFromInventory(Inventory inv, string resourceName, int amount = 1)
    {
        if (amount <= 0) return false;
        if (!recipes.ContainsKey(resourceName)) return false;
        if (!CanAccept(resourceName)) return false;

        if (inv.RemoveResource(resourceName, amount))
        {
            if (currentInput == null)
            {
                currentInput = resourceName;
                currentOutput = recipes[resourceName];
            }

            inputCount += amount;

            // Jeśli piec stał – uruchom
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

    // Opcjonalnie: wyczyść typ wsadu, gdy piec pusty (wsad + wynik)
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
}
