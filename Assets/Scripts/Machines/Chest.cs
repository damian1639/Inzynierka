using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class Chest : MonoBehaviour, IConveyorConsumer
{
    [Header("Wejście z taśmy")]
    public Transform intakePoint;   // pusty obiekt przy krawędzi skrzyni

    [Header("UI skrzyni")]
    public ChestUI chestUI;         // panel ChestPanel (z ChestUI)

    private Inventory _inventory;

    private void Awake()
    {
        _inventory = GetComponent<Inventory>();
    }

    // ======== OTWIERANIE / ZAMYKANIE (dla ChestInteractor) ========

    // wołane z ChestInteractor, gdy naciśniesz E na skrzyni
    public void Open(Inventory playerInventory, InventoryUI playerInventoryUI)
    {
        if (chestUI == null)
        {
            Debug.LogWarning("Chest: brak przypisanego ChestUI.");
            return;
        }

        chestUI.Open(_inventory, playerInventory, playerInventoryUI);
    }

    public void Close()
    {
        if (chestUI != null)
            chestUI.Close();
    }

    // opcjonalnie – jakbyś kiedyś potrzebował dostępu do Inventory
    public Inventory GetInventory() => _inventory;

    // ======== WEJŚCIE Z TAŚMOCIĄGU (IConveyorConsumer) ========

    // Ta metoda jest wołana przez taśmociąg, gdy item wjeżdża na skrzynię
    public bool TryAcceptItem(string resourceId)
    {
        if (_inventory == null || string.IsNullOrEmpty(resourceId))
            return false;

        _inventory.AddResource(resourceId, 1);
        return true;
    }

    // Ta metoda jest wołana, gdy taśmociąg pyta "gdzie jest wlot?"
    public Vector3 GetIntakePosition()
    {
        return intakePoint != null ? intakePoint.position : transform.position;
    }
}
