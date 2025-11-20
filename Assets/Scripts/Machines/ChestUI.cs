using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestUI : MonoBehaviour
{
    [System.Serializable]
    public class SlotRefs
    {
        public Image background;          // obrazek tła (Slot)
        public TextMeshProUGUI countText; // tekst w slocie
    }

    [Header("UI")]
    public GameObject rootPanel;   // ChestPanel
    public SlotRefs[] slots = new SlotRefs[20];

    [Header("Kolory")]
    public Color emptyColor = new Color(1, 1, 1, 0.2f);
    public Color filledColor = Color.white;

    private Inventory chestInventory;
    private Inventory playerInventory;
    private InventoryUI playerInventoryUI;

    // kolejność zasobów wyświetlanych w slotach: index 0 → slot 0, itd.
    private readonly List<string> displayedResources = new List<string>();

    // zaznaczenie
    private enum SelectionOwner { None, Player, Chest }
    private SelectionOwner selectionOwner = SelectionOwner.None;
    private int selectedChestSlot = -1;
    private string selectedChestResource = null;
    private string selectedPlayerResource = null;

    private void Awake()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        EnsureClickableSlots();
    }

    // ==== API wołane z Chest.cs ====
    public void Open(Inventory chestInv, Inventory playerInv, InventoryUI playerInvUI)
    {
        chestInventory = chestInv;
        playerInventory = playerInv;
        playerInventoryUI = playerInvUI;

        if (rootPanel != null)
            rootPanel.SetActive(true);

        // event: gdy zawartość skrzyni się zmieni -> odśwież
        if (chestInventory != null)
        {
            chestInventory.OnChanged -= OnChestChanged;
            chestInventory.OnChanged += OnChestChanged;
        }

        // event: gdy klikniesz slot w ekwipunku -> ChestUI ma zareagować
        if (playerInventoryUI != null)
        {
            playerInventoryUI.OnSlotSelected -= OnPlayerSlotSelected;
            playerInventoryUI.OnSlotSelected += OnPlayerSlotSelected;
        }

        selectionOwner = SelectionOwner.None;
        selectedChestSlot = -1;
        selectedChestResource = null;
        selectedPlayerResource = null;

        RefreshAll();
    }

    public void Close()
    {
        if (rootPanel != null)
            rootPanel.SetActive(false);

        if (chestInventory != null)
            chestInventory.OnChanged -= OnChestChanged;

        if (playerInventoryUI != null)
            playerInventoryUI.OnSlotSelected -= OnPlayerSlotSelected;

        selectionOwner = SelectionOwner.None;
        selectedChestSlot = -1;
        selectedChestResource = null;
        selectedPlayerResource = null;
    }

    private void OnChestChanged()
    {
        RefreshChest();
    }

    // ==== odświeżanie ====
    private void RefreshAll()
    {
        RefreshChest();
        if (playerInventoryUI != null && playerInventory != null)
            playerInventoryUI.Refresh(playerInventory);
    }

    private void RefreshChest()
    {
        if (chestInventory == null) return;

        displayedResources.Clear();

        // 1) wyczyść wszystkie sloty
        for (int i = 0; i < slots.Length; i++)
            SetChestSlotEmpty(i);

        // 2) wpisz rzeczy z Inventory do slotów po kolei
        int index = 0;
        foreach (var kv in chestInventory.GetAll())
        {
            if (index >= slots.Length) break;
            string resId = kv.Key;
            int count = kv.Value;
            if (count <= 0) continue;

            displayedResources.Add(resId);          // index listy = index slota
            SetChestSlotFilled(index, resId, count);
            index++;
        }

        RefreshChestSelectionVisual();
    }

    private void SetChestSlotEmpty(int i)
    {
        if (i < 0 || i >= slots.Length || slots[i] == null) return;
        var s = slots[i];
        if (s.background != null) s.background.color = emptyColor;
        if (s.countText != null) s.countText.text = "";
    }

    private void SetChestSlotFilled(int i, string resName, int count)
    {
        if (i < 0 || i >= slots.Length || slots[i] == null) return;
        var s = slots[i];
        if (s.background != null) s.background.color = filledColor;
        if (s.countText != null)
            s.countText.text = count > 1 ? $"{resName}\nx{count}" : resName;
    }

    private void RefreshChestSelectionVisual()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i]?.background == null) continue;
            var outline = slots[i].background.GetComponent<Outline>();
            if (outline == null) outline = slots[i].background.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.cyan;
            outline.effectDistance = new Vector2(2, -2);
            outline.enabled = (selectionOwner == SelectionOwner.Chest && i == selectedChestSlot);
        }
    }

    private void EnsureClickableSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i]?.background == null) continue;
            var go = slots[i].background.gameObject;
            var click = go.GetComponent<ChestSlotClick>();
            if (click == null) click = go.AddComponent<ChestSlotClick>();
            click.chestUI = this;
        }
    }

    // ==== klik w slot skrzyni (z ChestSlotClick) ====
    public void OnChestSlotClicked(int index)
    {
        if (chestInventory == null || playerInventory == null) return;
        if (index < 0 || index >= slots.Length) return;

        string resInSlot = index < displayedResources.Count ? displayedResources[index] : null;

        if (string.IsNullOrEmpty(resInSlot))
        {
            // klik w PUSTY slot skrzyni
            if (selectionOwner == SelectionOwner.Player && !string.IsNullOrEmpty(selectedPlayerResource))
            {
                // przenosimy 1 szt. z gracza do skrzyni
                MoveOne(playerInventory, chestInventory, selectedPlayerResource);
            }

            selectionOwner = SelectionOwner.None;
            selectedChestSlot = -1;
            selectedChestResource = null;
        }
        else
        {
            // klik w slot z itemem
            if (selectionOwner == SelectionOwner.None)
            {
                selectionOwner = SelectionOwner.Chest;
                selectedChestSlot = index;
                selectedChestResource = resInSlot;
            }
            else if (selectionOwner == SelectionOwner.Chest && selectedChestSlot == index)
            {
                // drugi klik ten sam slot – odznacz
                selectionOwner = SelectionOwner.None;
                selectedChestSlot = -1;
                selectedChestResource = null;
            }
            else if (selectionOwner == SelectionOwner.Player && !string.IsNullOrEmpty(selectedPlayerResource))
            {
                // zaznaczony był slot gracza → próbujemy do skrzyni
                MoveOne(playerInventory, chestInventory, selectedPlayerResource);
                selectionOwner = SelectionOwner.None;
            }
        }

        RefreshAll();
    }

    // ==== reagowanie na kliknięcia w ekwipunku gracza ====
    private void OnPlayerSlotSelected(string resId)
    {
        if (playerInventory == null || chestInventory == null) return;

        if (string.IsNullOrEmpty(resId))
        {
            // klik pustego slota w ekwipunku
            if (selectionOwner == SelectionOwner.Chest && !string.IsNullOrEmpty(selectedChestResource))
            {
                // zaznaczony był slot skrzyni → przenieś 1 szt. do ekwipunku
                MoveOne(chestInventory, playerInventory, selectedChestResource);
            }

            selectionOwner = SelectionOwner.None;
            selectedChestSlot = -1;
            selectedChestResource = null;
            selectedPlayerResource = null;

            RefreshAll();
            return;
        }

        // klik w slot z itemem w ekwipunku
        if (selectionOwner == SelectionOwner.None)
        {
            selectionOwner = SelectionOwner.Player;
            selectedPlayerResource = resId;
        }
        else if (selectionOwner == SelectionOwner.Player && selectedPlayerResource == resId)
        {
            // drugi klik ten sam – odznacz
            selectionOwner = SelectionOwner.None;
            selectedPlayerResource = null;
        }
        else if (selectionOwner == SelectionOwner.Chest && !string.IsNullOrEmpty(selectedChestResource))
        {
            // zaznaczona skrzynia, klik w slot ekwipunku → też przenieś ze skrzyni
            MoveOne(chestInventory, playerInventory, selectedChestResource);
            selectionOwner = SelectionOwner.None;
            selectedChestSlot = -1;
            selectedChestResource = null;
        }

        RefreshAll();
    }

    private void MoveOne(Inventory from, Inventory to, string resId)
    {
        if (from == null || to == null || string.IsNullOrEmpty(resId)) return;
        if (from.GetResourceCount(resId) <= 0) return;

        if (from.RemoveResource(resId, 1))
            to.AddResource(resId, 1);
    }
}
