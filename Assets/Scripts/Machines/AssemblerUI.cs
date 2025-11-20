using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AssemblerUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject rootPanel;      // AssemblerPanel
    public Assembler assembler;       // ustawiane z interaktora
    public Inventory playerInventory; // Inventory z Playera
    public InventoryUI playerInventoryUI; // InventoryUI z PanelEkwipunku

    [Header("Teksty")]
    public TextMeshProUGUI inputAText;
    public TextMeshProUGUI inputBText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI outputText;

    [Header("Przyciski")]
    public Button btnTakeOutput;  // "Zabierz wynik"

    private void Start()
    {
        if (btnTakeOutput)
            btnTakeOutput.onClick.AddListener(OnTakeOutput);

        SetVisible(false);
        Refresh();
    }

    private void Update()
    {
        if (rootPanel != null && rootPanel.activeSelf)
            Refresh();
    }

    public void SetVisible(bool visible)
    {
        if (rootPanel != null)
            rootPanel.SetActive(visible);
    }

    // Otwierane z interaktora
    public void Open(Assembler asm, Inventory playerInv, InventoryUI invUI)
    {
        assembler = asm;
        playerInventory = playerInv;
        playerInventoryUI = invUI;

        SetVisible(true);
        Refresh();
    }

    public void Close()
    {
        SetVisible(false);
    }

    // ==== kliknięcie w pole A/B – wołane z AssemblerInputSlot ====

    public void TryInsertFromSelectedA()
    {
        TryInsertFromSelected(isSlotA: true);
    }

    public void TryInsertFromSelectedB()
    {
        TryInsertFromSelected(isSlotA: false);
    }

    private void TryInsertFromSelected(bool isSlotA)
    {
        if (assembler == null || playerInventory == null || playerInventoryUI == null)
            return;

        string res = playerInventoryUI.selectedResource;
        if (string.IsNullOrEmpty(res))
        {
            Debug.Log("Assembler: nic nie zaznaczono w ekwipunku.");
            return;
        }

        bool ok = false;
        if (isSlotA)
            ok = assembler.InsertInputA(playerInventory, res, 1);
        else
            ok = assembler.InsertInputB(playerInventory, res, 1);

        if (!ok)
        {
            Debug.Log("Assembler: nie można włożyć tego przedmiotu do wybranego slotu.");
        }
        else
        {
            // po udanym włożeniu od razu próbujemy zacząć craftowanie
            assembler.StartCrafting();
        }

        Refresh();
    }

    // ==== zabieranie wyjścia ====

    private void OnTakeOutput()
    {
        if (assembler == null || playerInventory == null) return;
        int taken = assembler.TakeAllOutputToInventory(playerInventory);
        if (taken > 0)
            Debug.Log($"Zabrano {taken}x {assembler.outputId} z assemblera.");
        Refresh();
    }

    // ==== odświeżanie widoku ====

    public void Refresh()
    {
        if (assembler == null)
        {
            if (inputAText) inputAText.text = "Input A: - x0";
            if (inputBText) inputBText.text = "Input B: - x0";
            if (progressText) progressText.text = "Progress: -";
            if (outputText) outputText.text = "Output: - x0";
            return;
        }

        string aName = assembler.currentAId ?? "-";
        string bName = assembler.currentBId ?? "-";
        string outName = assembler.outputId ?? "-";

        if (inputAText) inputAText.text = $"{aName}\nx{assembler.countA}";
        if (inputBText) inputBText.text = $"{bName}\nx{assembler.countB}";

        if (progressText)
        {
            string p = assembler.isWorking ? Mathf.RoundToInt(assembler.progress01 * 100f) + "%" : "-";
            progressText.text = $"Progress: {p}";
        }

        if (outputText) outputText.text = $"{outName}\nx{assembler.outputCount}";
    }
}
