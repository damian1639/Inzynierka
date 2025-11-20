using UnityEngine;
using UnityEngine.EventSystems;

public class ChestSlotClick : MonoBehaviour, IPointerClickHandler
{
    public ChestUI chestUI;   // ustawiany z ChestUI.EnsureClickableSlots

    public void OnPointerClick(PointerEventData eventData)
    {
        if (chestUI == null) return;

        // background (Image) jest na obiekcie Slot,
        // więc index to pozycja TEGO obiektu w SlotsGrid
        int index = transform.GetSiblingIndex();
        chestUI.OnChestSlotClicked(index);
    }
}
