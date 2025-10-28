using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    public string resourceName = "Stone"; // nazwa surowca
    public int amount = 1; // ile jednostek dodaje

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // znajdź skrypt od ekwipunku (dodamy później)
            Inventory inventory = other.GetComponent<Inventory>();
            if (inventory != null)
            {
                inventory.AddResource(resourceName, amount);
                Debug.Log($"Gracz zebrał {amount} x {resourceName}");
            }

            // obiekt NIE znika (bo to złoże)
            // w przyszłości możemy dodać np. "cooldown" lub efekt kopania
        }
    }
}
