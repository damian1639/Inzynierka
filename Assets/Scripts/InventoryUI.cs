using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [System.Serializable]
    public class SlotRefs
    {
        public Image background;          // tło slota
        public Image icon;                // ikonka surowca
        public TextMeshProUGUI countText; // ilość (TMP)
    }

    [System.Serializable]
    public class ResourceIcon
    {
        public string resourceName = "Stone";
        public Sprite sprite; // przypisz w Inspectorze
    }

    [Header("Sloty (1–9)")]
    public SlotRefs[] slots = new SlotRefs[9];

    [Header("Ikony surowców")]
    public List<ResourceIcon> resourceIcons = new List<ResourceIcon>();

    [Header("Kolory")]
    public Color emptyColor = new Color(1, 1, 1, 0.25f);
    public Color filledColor = Color.white;
    public Color selectedColor = new Color(0.5f, 0.8f, 1f, 1f); // podświetlenie zaznaczenia

    // mapowanie surowców → sloty
    [HideInInspector] public Dictionary<string, int> resourceToSlot = new Dictionary<string, int>();
    private Dictionary<string, Sprite> iconMap = new Dictionary<string, Sprite>();

    // zaznaczenie slotu
    [HideInInspector] public int selectedSlot = -1;
    [HideInInspector] public string selectedResource = null;

    private Inventory boundInventory;

    private void Awake()
    {
        // Zmapuj nazwy surowców na ich sprite'y
        iconMap.Clear();
        foreach (var ri in resourceIcons)
        {
            if (ri != null && !string.IsNullOrEmpty(ri.resourceName))
                iconMap[ri.resourceName] = ri.sprite;
        }

        EnsureClickableSlots();
    }

    public void Bind(Inventory inventory)
    {
        boundInventory = inventory;
        inventory.OnChanged -= () => Refresh(inventory);
        inventory.OnChanged += () => Refresh(inventory);
        Refresh(inventory);
    }

    // Główne odświeżanie UI
    public void Refresh(Inventory inventory)
    {
        if (inventory == null) return;
        var all = inventory.GetAll();

        // 1) wyczyść wszystkie sloty
        for (int i = 0; i < slots.Length; i++)
            SetSlotEmpty(i);

        // 2) usuń nieużywane przydziały
        List<string> toRemove = new List<string>();
        foreach (var kv in resourceToSlot)
        {
            if (!all.ContainsKey(kv.Key) || all[kv.Key] <= 0)
                toRemove.Add(kv.Key);
        }
        foreach (var k in toRemove) resourceToSlot.Remove(k);

        // 3) przypisz surowce do slotów
        foreach (var kv in all)
        {
            string resource = kv.Key;
            int count = kv.Value;
            if (count <= 0) continue;

            if (!resourceToSlot.TryGetValue(resource, out int slotIndex))
            {
                slotIndex = FindFirstFreeSlot();
                if (slotIndex == -1) continue;
                resourceToSlot[resource] = slotIndex;
            }

            SetSlotFilled(slotIndex, resource, count);
        }

        RefreshSelectedVisual();
    }

    private int FindFirstFreeSlot()
    {
        HashSet<int> taken = new HashSet<int>(resourceToSlot.Values);
        for (int i = 0; i < slots.Length; i++)
            if (!taken.Contains(i)) return i;
        return -1;
    }

    private void SetSlotEmpty(int i)
    {
        if (i < 0 || i >= slots.Length || slots[i] == null) return;
        var s = slots[i];
        if (s.background) s.background.color = emptyColor;
        if (s.icon) { s.icon.enabled = false; s.icon.sprite = null; }
        if (s.countText) s.countText.text = "";
    }

    private void SetSlotFilled(int i, string resourceName, int count)
    {
        if (i < 0 || i >= slots.Length || slots[i] == null) return;
        var s = slots[i];

        if (s.background) s.background.color = filledColor;
        if (s.icon)
        {
            s.icon.enabled = true;
            s.icon.sprite = iconMap.TryGetValue(resourceName, out var sp) ? sp : null;
        }

        if (s.countText)
            s.countText.text = count > 1 ? $"{resourceName}\n×{count}" : resourceName;
    }

    // ---------------- ZAZNACZANIE SLOTU ----------------
    public void SelectSlot(int slotIndex)
    {
        selectedSlot = slotIndex;
        selectedResource = null;

        foreach (var kv in resourceToSlot)
        {
            if (kv.Value == slotIndex)
            {
                selectedResource = kv.Key;
                break;
            }
        }
        RefreshSelectedVisual();
    }

    private void RefreshSelectedVisual()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i]?.background == null) continue;
            var outline = slots[i].background.GetComponent<Outline>();
            if (outline == null) outline = slots[i].background.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.cyan;
            outline.effectDistance = new Vector2(2, -2);
            outline.enabled = (i == selectedSlot);
        }
    }

    private void EnsureClickableSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i]?.background == null) continue;
            var go = slots[i].background.gameObject;
            var click = go.GetComponent<InventorySlotClick>();
            if (click == null) click = go.AddComponent<InventorySlotClick>();
            click.slotIndex = i;
            click.inventoryUI = this;
        }
    }

    // ---------------- INTERAKCJA Z PIECU ----------------
    public bool TryInsertSelected(InventoryUI invUI, string resourceName)
    {
        if (boundInventory == null) return false;

        if (boundInventory.GetResourceCount(resourceName) <= 0)
            return false;

        var furnaceUI = FindObjectOfType<FurnaceUI>();
        if (furnaceUI == null) return false;

        bool ok = furnaceUI.furnace.InsertFromInventory(boundInventory, resourceName, 1);
        if (ok)
        {
            furnaceUI.Refresh();
            Refresh(boundInventory);

            if (boundInventory.GetResourceCount(resourceName) <= 0)
            {
                invUI.selectedResource = null;
                invUI.selectedSlot = -1;
                invUI.Refresh(boundInventory);
            }
        }
        return ok;
    }
}
