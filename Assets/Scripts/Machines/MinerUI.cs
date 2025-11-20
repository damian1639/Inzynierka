using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinerUI : MonoBehaviour
{
    [Header("Referencje")]
    public GameObject rootPanel;      // GameObject panelu
    public Miner miner;               // ustawiane przy Open()
    public Inventory playerInventory; // Inventory gracza

    [Header("Teksty")]
    public TextMeshProUGUI titleText;    // np. "Miner"
    public TextMeshProUGUI targetText;   // np. "Target: Stone"
    public TextMeshProUGUI progressText; // "Progress: 32%"
    public TextMeshProUGUI outputText;   // "Output: Stone x12"

    [Header("Przyciski")]
    public Button btnTakeAll;

    private void Start()
    {
        if (btnTakeAll) btnTakeAll.onClick.AddListener(TakeAll);
        SetVisible(false);
        DrawEmpty();
    }

    private void Update()
    {
        if (rootPanel != null && rootPanel.activeSelf) Refresh();
    }

    public void Open(Miner m)
    {
        miner = m;
        SetVisible(true);
        Refresh();
    }

    public void Close()
    {
        SetVisible(false);
        miner = null;
    }

    public void SetVisible(bool on) => rootPanel?.SetActive(on);

    private void DrawEmpty()
    {
        if (titleText)   titleText.text   = "Miner";
        if (targetText)  targetText.text  = "Target: -";
        if (progressText)progressText.text= "Progress: -";
        if (outputText)  outputText.text  = "Output: - x0";
    }

    public void Refresh()
    {
        if (miner == null) { DrawEmpty(); return; }

        string target = string.IsNullOrEmpty(miner.targetResourceId) ? "-" : miner.targetResourceId;
        if (titleText)    titleText.text    = "Miner";
        if (targetText)   targetText.text   = $"Target: {target}";
        if (progressText) progressText.text = $"Progress: {(miner.isWorking ? Mathf.RoundToInt(miner.progress01 * 100f) + "%" : "-")}";
        if (outputText)   outputText.text   = $"Output: {target}\nx{miner.outputCount}";
    }

    private void TakeAll()
    {
        if (miner == null || playerInventory == null) return;
        int taken = miner.TakeAllToInventory(playerInventory);
        if (taken > 0) Debug.Log($"Miner: zabrano {taken}x {miner.targetResourceId}");
        Refresh();
    }
}
