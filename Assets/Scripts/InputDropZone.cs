using UnityEngine;
using UnityEngine.EventSystems;

public class InputDropZone : MonoBehaviour, IPointerClickHandler
{
    public FurnaceUI furnaceUI;     // przypnij w Inspectorze (FurnacePanel z FurnaceUI)
    public InventoryUI inventoryUI; // przypnij w Inspectorze (PanelEkwipunku z InventoryUI)

    public void OnPointerClick(PointerEventData eventData)
    {
        if (furnaceUI == null || inventoryUI == null) return;

        // Log diagnostyczny (pomaga, gdyby nadal coś nie łapało)
        Debug.Log($"[InputDropZone] furnace={(furnaceUI.furnace ? furnaceUI.furnace.name : "NULL")}, selected={inventoryUI.selectedResource}");

        var res = inventoryUI.selectedResource;
        if (string.IsNullOrEmpty(res)) return; // nic nie wybrane

        bool ok = furnaceUI.InsertSelected(res);
        if (!ok)
            Debug.Log("Nie można włożyć: brak surowca / zła recepta / inny typ wsadu w piecu.");
    }
}
