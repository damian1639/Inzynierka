using UnityEngine;

public class ConveyorBelt : MonoBehaviour, IConveyorConsumer
{
    [Header("Punkty pasa")]
    public Transform intakePoint;      // skąd przyjmujemy (od Minera / poprzedniego pasa)
    public Transform outtakePoint;     // dokąd wysyłamy (do kolejnego pasa / pieca)

    [Header("Ruch")]
    public float moveTime = 0.6f;      // czas przejazdu z intake do outtake

    [Header("Szukany odbiorca z przodu")]
    public LayerMask consumerMask;     // ustaw na Buildable
    public float consumerSearchRadius = 0.4f;

    [Header("Wizual itemu")]
    public float itemHeight = 0.1f;
    public Vector3 itemScale = new Vector3(0.3f, 0.3f, 0.3f);

    private string _currentItemId = null;   // id surowca na pasie
    private float _t = 0f;                  // 0..1 postęp na pasie
    private GameObject _visualItem = null;  // klocek wizualny
    private IConveyorConsumer _forwardConsumer = null;

    private void Awake()
    {
        EnsurePoints();
    }

    private void EnsurePoints()
    {
        // NIE nadpisujemy ręcznie ustawionych punktów – tylko tworzymy, jeśli brak
        if (intakePoint == null)
        {
            var go = new GameObject("IntakePoint");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.05f, -0.5f);
            intakePoint = go.transform;
        }

        if (outtakePoint == null)
        {
            var go = new GameObject("OuttakePoint");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.05f, 0.5f);
            outtakePoint = go.transform;
        }
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(_currentItemId)) return;

        // jeśli odbiorca z przodu został zniszczony – wyczyść cache
        if (_forwardConsumer is MonoBehaviour mb && !mb)
        {
            _forwardConsumer = null;
        }

        // upewnij się, że mamy klocek wizualny
        if (_visualItem == null)
        {
            _visualItem = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _visualItem.name = $"BeltItem_{_currentItemId}";
            Destroy(_visualItem.GetComponent<Collider>());
            _visualItem.transform.localScale = itemScale;
        }

        // ruch od intake do outtake
        _t += Time.deltaTime / moveTime;
        if (_t > 1f) _t = 1f;

        Vector3 pos = Vector3.Lerp(intakePoint.position, outtakePoint.position, _t);
        pos.y += itemHeight;
        _visualItem.transform.position = pos;

        // doszliśmy do końca pasa → próbuj oddać dalej
        if (_t >= 1f)
        {
            // jeśli nie mamy odbiorcy albo poprzedni był ghostem – poszukaj na nowo
            if (_forwardConsumer == null)
                CacheForwardConsumer();

            bool delivered = false;

            if (_forwardConsumer != null)
            {
                try
                {
                    delivered = _forwardConsumer.TryAcceptItem(_currentItemId);
                }
                catch
                {
                    // np. MissingReferenceException po zniszczeniu obiektu
                    delivered = false;
                    _forwardConsumer = null;
                }
            }

            if (delivered)
            {
                // udało się oddać dalej – pas pusty
                Destroy(_visualItem);
                _visualItem = null;
                _currentItemId = null;
                _t = 0f;
            }
            else
            {
                // nie udało się – spróbujemy ponownie później, ale upewnij się że będziemy mogli prze-cache'ować
                if (_forwardConsumer != null)
                {
                    // jeśli odbiorca nie przyjmuje (np. był duchem) – zapomnij go,
                    // dzięki temu po postawieniu prawdziwego pieca znajdziemy nowy
                    _forwardConsumer = null;
                }
            }
        }
    }

    // ============= IConveyorConsumer =============
    bool IConveyorConsumer.TryAcceptItem(string resourceId)
    {
        // pas pełny? – nie przyjmujemy kolejnego
        if (!string.IsNullOrEmpty(_currentItemId))
            return false;

        _currentItemId = resourceId;
        _t = 0f;
        // wizual utworzy się w Update
        return true;
    }

    Vector3 IConveyorConsumer.GetIntakePosition()
    {
        return intakePoint != null ? intakePoint.position : transform.position;
    }

    // ============= szukanie odbiorcy z przodu =============
    private void CacheForwardConsumer()
    {
        _forwardConsumer = null;
        if (outtakePoint == null) return;

        var hits = Physics.OverlapSphere(
            outtakePoint.position,
            consumerSearchRadius,
            consumerMask,
            QueryTriggerInteraction.Collide
        );

        foreach (var h in hits)
        {
            var behaviours = h.GetComponentsInParent<MonoBehaviour>(true);
            foreach (var b in behaviours)
            {
                // pomijamy sam siebie
                if (b == this) continue;

                // pomijamy wyłączone skrypty
                if (!b.isActiveAndEnabled) continue;

                // pomijamy DUCHY (root o nazwie kończącej się na "_GHOST")
                var root = b.transform.root;
                if (root != null && root.name.EndsWith("_GHOST")) continue;

                if (b is IConveyorConsumer cons)
                {
                    _forwardConsumer = cons;
                    return;
                }
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        EnsurePoints();

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            intakePoint.position + Vector3.up * 0.02f,
            outtakePoint.position + Vector3.up * 0.02f
        );

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(intakePoint.position + Vector3.up * 0.05f, 0.05f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(outtakePoint.position + Vector3.up * 0.05f, 0.05f);
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawWireSphere(outtakePoint.position, consumerSearchRadius);
    }
#endif
}
