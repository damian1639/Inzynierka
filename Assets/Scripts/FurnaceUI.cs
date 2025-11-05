using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FurnaceUI : MonoBehaviour
{
    [Header("Referencje")]
    public GameObject rootPanel;          // FurnacePanel (GameObject do włączania/wyłączania)
    public Furnace furnace;               // będzie ustawione w Open() lub na starcie (auto-bind)
    public Inventory playerInventory;     // Inventory z Playera

    [Header("Auto-bind na starcie")]
    public Furnace initialFurnace;        // (opcjonalnie) przeciągnij piec z Hierarchy
    public bool autoBindOnStart = true;   // jeśli true, na starcie spróbujemy od razu podpiąć piec

    [Header("Teksty")]
    public TextMeshProUGUI inputText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI outputText;

    [Header("Przyciski (opcjonalne)")]
    public Button btnInsertStone;
    public Button btnInsertCopper;
    public Button btnTakeOutput;

    private void Start()
    {
        if (btnInsertStone) btnInsertStone.onClick.AddListener(() => Insert("Stone"));
        if (btnInsertCopper) btnInsertCopper.onClick.AddListener(() => Insert("Copper"));
        if (btnTakeOutput)   btnTakeOutput.onClick.AddListener(TakeOutput);

        // Panel na starcie może pozostać schowany
        SetVisible(false);

        // Ustaw domyślne myślniki, żeby UI nie było puste
        if (inputText)    inputText.text    = "Input: - x0";
        if (progressText) progressText.text = "Progress: -";
        if (outputText)   outputText.text   = "Output: - x0";

        // >>> AUTO-BIND PIECA NA STARCIE <<<
        if (autoBindOnStart)
        {
            if (furnace == null) furnace = initialFurnace;
            if (furnace == null) furnace = FindObjectOfType<Furnace>();

            if (furnace != null)
            {
                // od razu odśwież dane (myślniki, x0, itd.), żeby po pierwszym otwarciu wszystko było gotowe
                Refresh();
                Debug.Log($"[FurnaceUI] Auto-bound to: {furnace.name}");
            }
            else
            {
                Debug.LogWarning("[FurnaceUI] Nie znaleziono żadnego Furnace w scenie podczas auto-bind.");
            }
        }
    }

    private void Update()
    {
        if (rootPanel != null && rootPanel.activeSelf)
            Refresh();
    }

    // --- Otwieranie / zamykanie panelu ---

    public void Open(Furnace f)
    {
        furnace = f;                 // ważne: ustawiamy piec
        SetVisible(true);
        Refresh();
        Debug.Log($"[FurnaceUI] OPEN for: {furnace.name}");
    }

    public void Close()
    {
        SetVisible(false);
        // UWAGA: nie zerujemy furnace — zostawiamy go, żeby po ponownym otwarciu było gotowe
        Debug.Log("[FurnaceUI] CLOSE");
    }

    public void SetVisible(bool on)
    {
        if (rootPanel) rootPanel.SetActive(on);
    }

    // --- Wkładanie przez przyciski (opcjonalne) ---

    private void Insert(string resourceName)
    {
        if (furnace == null || playerInventory == null) return;

        bool ok = furnace.InsertFromInventory(playerInventory, resourceName, 1);
        if (!ok)
            Debug.Log("Nie można włożyć: brak surowca, zła recepta albo w piecu inny wsad.");
        Refresh();
    }

    // --- Wkładanie z InputDropZone (klik w pole wsadu) ---

    public bool InsertSelected(string resourceName)
    {
        if (string.IsNullOrEmpty(resourceName)) return false;
        if (furnace == null || playerInventory == null)
        {
            Debug.LogWarning("[FurnaceUI] InsertSelected: furnace lub inventory jest NULL.");
            return false;
        }

        bool ok = furnace.InsertFromInventory(playerInventory, resourceName, 1);
        if (ok) Refresh();
        return ok;
    }

    // --- Zabieranie wyniku ---

    private void TakeOutput()
    {
        if (furnace == null || playerInventory == null) return;

        int taken = furnace.TakeAllOutputToInventory(playerInventory);
        if (taken > 0) Debug.Log($"Zabrano {taken}x {furnace.currentOutput} z pieca.");
        furnace.TryResetTypeIfEmpty();
        Refresh();
    }

    // --- Odświeżanie UI ---

    public void Refresh()
    {
        if (furnace == null)
        {
            if (inputText)    inputText.text    = "Input: - x0";
            if (progressText) progressText.text = "Progress: -";
            if (outputText)   outputText.text   = "Output: - x0";
            return;
        }

        string inName  = furnace.currentInput  ?? "-";
        string outName = furnace.currentOutput ?? "-";

        if (inputText)    inputText.text    = $"{inName}\nx{furnace.inputCount}";
        if (progressText) progressText.text = $"Progress: {(furnace.isWorking ? Mathf.RoundToInt(furnace.progress01 * 100f) + "%" : "-")}";
        if (outputText)   outputText.text   = $"{outName}\nx{furnace.outputCount}";
    }
}
