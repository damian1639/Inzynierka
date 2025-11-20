using UnityEngine;

public class DummyBeltToFurnace : MonoBehaviour
{
    public Furnace furnace;
    public string resourceId = "Stone";  // albo "Copper", zależnie od recepty
    public float interval = 1f;          // co ile sekund „wpycha” 1 item

    private float _timer = 0f;

    private void Update()
    {
        if (furnace == null) return;

        _timer += Time.deltaTime;
        if (_timer >= interval)
        {
            _timer = 0f;

            bool ok = furnace.TryAcceptItem(resourceId);
            if (ok)
                Debug.Log($"[DummyBelt] wcisnąłem 1x {resourceId} do pieca.");
            else
                Debug.Log("[DummyBelt] piec NIE przyjął itemu (zła recepta / inny typ / pełny?).");
        }
    }
}
