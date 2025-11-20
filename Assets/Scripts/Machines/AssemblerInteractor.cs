using UnityEngine;

public class AssemblerInteractor : MonoBehaviour
{
    [Header("Referencje")]
    public Transform player;              // Player (Transform)
    public Camera cam;                    // główna kamera
    public Inventory playerInventory;     // Inventory z gracza
    public InventoryUI playerInventoryUI; // InventoryUI (PanelEkwipunek)
    public AssemblerUI assemblerUI;       // AssemblerPanel

    [Header("Zasięg")]
    public float interactDistance = 3f;

    private Assembler _hoveredAssembler;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
        if (cam == null || assemblerUI == null || playerInventory == null || player == null)
            return;

        // 1) Szukamy assemblera pod kursorem
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        _hoveredAssembler = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
        {
            _hoveredAssembler = hit.collider.GetComponentInParent<Assembler>();
        }

        // 2) Reakcja na E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_hoveredAssembler != null)
            {
                float dist = Vector3.Distance(player.position, _hoveredAssembler.transform.position);
                if (dist <= interactDistance)
                {
                    assemblerUI.Open(_hoveredAssembler, playerInventory, playerInventoryUI);
                }
                else
                {
                    Debug.Log("Podejdź bliżej do assemblera (E).");
                }
            }
            else
            {
                assemblerUI.Close();
            }
        }
    }
}
