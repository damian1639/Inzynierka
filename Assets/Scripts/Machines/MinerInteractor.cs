using UnityEngine;

public class MinerInteractor : MonoBehaviour
{
    public Transform player;
    public float interactDistance = 3f;
    public MinerUI minerUI;

    private Miner _hovered;

    void Update()
    {
        // celuj kursorem
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        _hovered = null;
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
            _hovered = hit.collider.GetComponentInParent<Miner>();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_hovered != null)
            {
                if (Vector3.Distance(player.position, _hovered.transform.position) <= interactDistance)
                    minerUI.Open(_hovered);
                else
                    Debug.Log("Podejdź bliżej do Minera (E).");
            }
            else
            {
                minerUI.Close();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            minerUI.Close();
    }
}
