using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [System.Serializable]
    public class SlotRefs
    {
        public Image background; // Image na obiekcie SlotX
        public Image icon;       // dziecko "Icon"
        public TextMeshProUGUI countText;   // dziecko "CountText"
    }

    [System.Serializable]
    public class ResourceIcon
    {
        public string resourceName = "Stone";
        public Sprite sprite; // przypisz sprite w Inspectorze (opcjonalnie)
    }

    [Header("Sloty (1-9 w kolejności)")]
    public SlotRefs[] slots = new SlotRefs[9];

    [Header("Ikony surowców (opcjonalnie)")]
    public List<ResourceIcon> resourceIcons = new List<ResourceIcon>();

    [Header("Wygląd")]
    public Color emptyColor = new Color(1, 1, 1, 0.25f);
    public Color filledColor = Color.white;

    // zapamiętujemy który surowiec siedzi w którym slocie
    private Dictionary<string, int> resourceToSlot = new Dictionary<string, int>();
    private Dictionary<string, Sprite> iconMap = new Dictionary<string, Sprite>();

    private void Awake()
    {
        // mapa nazw surowców na sprite
        iconMap.Clear();
        foreach (var ri in resourceIcons)
        {
            if (ri != null && !string.IsNullOrEmpty(ri.resourceName))
                iconMap[ri.resourceName] = ri.sprite;
        }
    }

    public void Bind(Inventory inventory)
    {
        // Podpinamy się pod zdarzenie (zrób to raz w Start PlayerInteractor albo tutaj z zewnątrz)
        inventory.OnChanged -= () => Refresh(inventory);
        inventory.OnChanged += () => Refresh(inventory);

        Refresh(inventory);
    }

    public void Refresh(Inventory inventory)
    {
        var all = inventory.GetAll();

        // 1) Wyczyść UI
        for (int i = 0; i < slots.Length; i++)
        {
            SetSlotEmpty(i);
        }

        // 2) Odśwież mapę przydziałów (usuwamy nieużywane)
        List<string> toRemove = new List<string>();
        foreach (var kv in resourceToSlot)
        {
            if (!all.ContainsKey(kv.Key) || all[kv.Key] <= 0)
                toRemove.Add(kv.Key);
        }
        foreach (var k in toRemove) resourceToSlot.Remove(k);

        // 3) Wyświetl każdy surowiec w jego slocie (lub przydziel pierwszy wolny)
        foreach (var kv in all)
        {
            string resource = kv.Key;
            int count = kv.Value;
            if (count <= 0) continue;

            int slotIndex;
            if (!resourceToSlot.TryGetValue(resource, out slotIndex))
            {
                slotIndex = FindFirstFreeSlot();
                if (slotIndex == -1) { Debug.LogWarning("Brak wolnych slotów w UI."); continue; }
                resourceToSlot[resource] = slotIndex;
            }

            SetSlotFilled(slotIndex, resource, count);
        }
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
        if (slots[i].background) slots[i].background.color = emptyColor;
        if (slots[i].icon) { slots[i].icon.enabled = false; slots[i].icon.sprite = null; }
        if (slots[i].countText) slots[i].countText.text = "";
    }

    private void SetSlotFilled(int i, string resourceName, int count)
    {
        if (i < 0 || i >= slots.Length || slots[i] == null) return;

        if (slots[i].background) slots[i].background.color = filledColor;

        if (slots[i].icon)
        {
            slots[i].icon.enabled = true;
            slots[i].icon.sprite = iconMap.TryGetValue(resourceName, out var sp) ? sp : null;
        }

        if (slots[i].countText)
           slots[i].countText.text = count > 1 ? $"{resourceName}\n×{count}" : resourceName;
;
    }
    
}
