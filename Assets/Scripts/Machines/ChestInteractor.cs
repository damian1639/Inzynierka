using UnityEngine;

public class ChestInteractor : MonoBehaviour
{
    [Header("Referencje")]
    public Transform player;          // Player (Transform)
    public Camera cam;                // Main Camera
    public Inventory playerInventory; // Inventory z gracza
    public InventoryUI playerInventoryUI; // InventoryUI z PanelEkwipunku
    public ChestUI chestUI;           // panel skrzyni (ChestPanel z ChestUI)

    [Header("Zasięg")]
    public float interactDistance = 3f;

    private Chest _hoveredChest;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        if (cam == null) return;

        // 1) Szukamy skrzyni pod kursorem
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        _hoveredChest = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
        {
            _hoveredChest = hit.collider.GetComponentInParent<Chest>();
        }

        // 2) Reakcja na E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_hoveredChest != null)
            {
                float dist = Vector3.Distance(player.position, _hoveredChest.transform.position);
                if (dist <= interactDistance)
                {
                    // otwórz skrzynię
                    _hoveredChest.chestUI = chestUI;  // na wszelki wypadek przypinamy UI
                    _hoveredChest.Open(playerInventory, playerInventoryUI);
                }
                else
                {
                    Debug.Log("Podejdź bliżej do skrzyni (E).");
                }
            }
            else
            {
                // E bez celowania w skrzynię → zamknij skrzynię
                if (chestUI != null)
                    chestUI.Close();
            }
        }
    }
}
