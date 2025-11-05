using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotClick : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex;           // ustawiany z InventoryUI
    public InventoryUI inventoryUI; // ustawiany z InventoryUI

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryUI != null)
            inventoryUI.SelectSlot(slotIndex);
    }
}
