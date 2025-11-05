using UnityEngine;
using System;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    private Dictionary<string, int> resources = new Dictionary<string, int>();

    public event Action OnChanged; // UI się pod to podepnie

    public void AddResource(string resourceName, int amount)
    {
        if (resources.ContainsKey(resourceName))
            resources[resourceName] += amount;
        else
            resources.Add(resourceName, amount);

        OnChanged?.Invoke();
        Debug.Log($"Dodano {amount}x {resourceName}. Teraz masz {resources[resourceName]}");
    }

    public int GetResourceCount(string resourceName)
    {
        return resources.TryGetValue(resourceName, out int v) ? v : 0;
    }

    public IReadOnlyDictionary<string, int> GetAll() => resources;

    public bool RemoveResource(string resourceName, int amount)
    {
        if (amount <= 0) return true;

        if (resources.TryGetValue(resourceName, out int current) && current >= amount)
        {
            resources[resourceName] = current - amount;
            if (resources[resourceName] <= 0)
                resources.Remove(resourceName);

            OnChanged?.Invoke();
            Debug.Log($"Zabrano {amount}x {resourceName}. Zostało {GetResourceCount(resourceName)}");
            return true;
        }
        return false;
    }
}
