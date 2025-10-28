using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Ustawienia interakcji")]
    public float interactDistance = 3f; // jak blisko trzeba podejść
    public LayerMask interactLayers = ~0; // na start wszystko

    [Header("Referencje")]
    public Transform player;        // ustaw Player z Hierarchy
    public Inventory inventory;     // ustaw komponent Inventory z gracza
    public InventoryUI inventoryUI; // ustaw skrypt UI (dodamy w kroku 4)

    private ResourceNode _hovered;
    
    void Start()
{
    if (inventoryUI != null && inventory != null)
        inventoryUI.Bind(inventory);
}

    void Update()
    {
        // 1) Ray spod kursora
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        ResourceNode hitNode = null;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactLayers, QueryTriggerInteraction.Ignore))
        {
            hitNode = hit.collider.GetComponent<ResourceNode>();
        }

        // 2) Zarządzanie podświetleniem
        if (_hovered != hitNode)
        {
            if (_hovered != null) _hovered.SetHighlight(false);
            _hovered = hitNode;
            if (_hovered != null) _hovered.SetHighlight(true);
        }

        // 3) Zbiór LPM, ale tylko jeśli jesteśmy dostatecznie blisko
        if (_hovered != null && Input.GetMouseButtonDown(0))
        {
            float dist = Vector3.Distance(player.position, _hovered.transform.position);
            if (dist <= interactDistance)
            {
                _hovered.Collect(inventory);
                if (inventoryUI != null) inventoryUI.Refresh(inventory);
            }
            else
            {
                Debug.Log("Za daleko, podejdź bliżej do złoża.");
            }
        }
    }

    private void OnDisable()
    {
        if (_hovered != null) _hovered.SetHighlight(false);
    }
}
