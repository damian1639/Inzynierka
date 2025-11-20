using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Miner : MonoBehaviour
{
    [Header("Parametry kopania")]
    public float timePerUnit = 2f;   // co ile sekund 1 surowiec
    public int bufferLimit = 100;    // maks. pojemność bufora

    [Header("Wyjście (do taśmy)")]
    public Transform outputPoint;
    public float outputSearchRadius = 0.35f;
    public LayerMask consumerMask;   // Buildable

    [Header("Stan")]
    public string targetResourceId = null;
    public int outputCount = 0;
    public float progress01 = 0f;
    public bool isWorking = false;

    private float _timer = 0f;
    private ResourceNode _boundNode;

    private IConveyorConsumer _cachedConsumer;
    private float _pushCooldown = 0f;

    // >>> NOWE <<<
    [Header("Auto-bind przy starcie (dla minerów postawionych ręcznie w scenie)")]
    public bool autoBindOnStart = true;
    public LayerMask resourceMask;   // ustaw na Resource

    private void Awake()
    {
        if (outputPoint == null)
        {
            var t = transform.Find("OutputPoint");
            if (t) outputPoint = t;
        }
    }

    private void Start()
    {
        // jeśli Miner nie jest jeszcze podłączony, a zaznaczyliśmy autoBindOnStart – spróbuj znaleźć rudę pod spodem
        if (autoBindOnStart && _boundNode == null)
        {
            TryAutoBindToResource();
        }
    }

    private void Update()
    {
        // --- kopanie ---
        if (isWorking && _boundNode != null && !string.IsNullOrEmpty(targetResourceId) && outputCount < bufferLimit)
        {
            _timer += Time.deltaTime;
            progress01 = Mathf.Clamp01(_timer / timePerUnit);

            if (_timer >= timePerUnit)
            {
                _timer = 0f;
                progress01 = 0f;

                if (!_boundNode.TryConsumeOne())
                {
                    isWorking = false;
                }
                else
                {
                    outputCount++;
                }
            }
        }

        // --- wypychanie do odbiorcy (taśma / kosz) ---
        if (outputCount > 0)
        {
            _pushCooldown -= Time.deltaTime;
            if (_pushCooldown <= 0f)
            {
                if (TryPushOne())
                    _pushCooldown = 0.05f;
                else
                    _pushCooldown = 0.25f;
            }
        }
    }

    public bool BindToNode(ResourceNode node)
    {
        if (node == null) return false;
        _boundNode = node;
        targetResourceId = node.resourceName;
        isWorking = true;
        _timer = 0f;
        progress01 = 0f;
        return true;
    }

    public int TakeAllToInventory(Inventory inv)
    {
        if (inv == null || outputCount <= 0 || string.IsNullOrEmpty(targetResourceId)) return 0;
        int taken = outputCount;
        inv.AddResource(targetResourceId, outputCount);
        outputCount = 0;
        return taken;
    }

    // ===== wypychanie =====
private bool TryPushOne()
{
    if (string.IsNullOrEmpty(targetResourceId) || outputCount <= 0) return false;

    // jeśli poprzedni odbiorca został zniszczony – wyczyść cache
    if (_cachedConsumer is MonoBehaviour mb && !mb)
    {
        _cachedConsumer = null;
    }

    if (_cachedConsumer == null)
        CacheConsumer();

    if (_cachedConsumer == null) return false;

    bool ok = false;
    try
    {
        ok = _cachedConsumer.TryAcceptItem(targetResourceId);
    }
    catch
    {
        ok = false;
        _cachedConsumer = null;
    }

    if (ok)
    {
        outputCount--;
        return true;
    }
    else
    {
        // np. ghost / odbiorca już nie przyjmuje – zapomnij go, żeby następnym razem szukać od nowa
        _cachedConsumer = null;
        return false;
    }
}

private void CacheConsumer()
{
    _cachedConsumer = null;
    if (!outputPoint) return;

    var hits = Physics.OverlapSphere(
        outputPoint.position,
        outputSearchRadius,
        consumerMask,
        QueryTriggerInteraction.Collide
    );

    foreach (var h in hits)
    {
        var behaviours = h.GetComponentsInParent<MonoBehaviour>(true);
        foreach (var b in behaviours)
        {
            // pomijamy wyłączone skrypty
            if (!b.isActiveAndEnabled) continue;

            // pomijamy DUCHY (root nazwa kończy się na "_GHOST")
            var root = b.transform.root;
            if (root != null && root.name.EndsWith("_GHOST")) continue;

            if (b is IConveyorConsumer cons)
            {
                _cachedConsumer = cons;
                return;
            }
        }
    }
}


    // >>> NOWE: auto-bind pod sobą <<<
    private void TryAutoBindToResource()
    {
        // strzelamy promieniem w dół tylko po warstwie Resource
        Vector3 origin = transform.position + Vector3.up * 2f;
        if (Physics.Raycast(origin, Vector3.down, out var hit, 5f, resourceMask, QueryTriggerInteraction.Collide))
        {
            var node = hit.collider.GetComponentInParent<ResourceNode>();
            if (node != null)
            {
                BindToNode(node);
                Debug.Log($"[Miner] AutoBind na starcie do złoża: {node.resourceName}");
            }
        }
    }
}
