using UnityEngine;

public class FurnaceInteractor : MonoBehaviour
{
    public Transform player;
    public float interactDistance = 3f;
    public FurnaceUI furnaceUI;

    private Furnace _hoveredFurnace;

    void Update()
    {
        // Traf piec pod kursorem (collider może być na dziecku)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        _hoveredFurnace = null;
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
            _hoveredFurnace = hit.collider.GetComponentInParent<Furnace>();

        // E = otwórz panel dla wskazanego pieca (albo zamknij, jeśli nie celujesz w piec)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_hoveredFurnace != null)
            {
                if (Vector3.Distance(player.position, _hoveredFurnace.transform.position) <= interactDistance)
                {
                    // ZAWSZE otwieraj dla aktualnie wskazanego pieca
                    furnaceUI.Open(_hoveredFurnace);
                }
                else
                {
                    Debug.Log("Podejdź bliżej do pieca (E).");
                }
            }
            else
            {
                furnaceUI.Close();
            }
        }

        // ESC zamyka panel
        if (Input.GetKeyDown(KeyCode.Escape))
            furnaceUI.Close();
    }
}
