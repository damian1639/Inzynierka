public static class InventorySelection
{
    public static Inventory selectedInventory = null;
    public static int selectedSlotIndex = -1;
    public static string selectedResourceId = null;

    public static void Clear()
    {
        selectedInventory = null;
        selectedSlotIndex = -1;
        selectedResourceId = null;
    }

    public static bool HasSelection => selectedInventory != null && !string.IsNullOrEmpty(selectedResourceId);
}
