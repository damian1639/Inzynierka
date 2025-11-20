using UnityEngine;

public class DummyConsumer : MonoBehaviour, IConveyorConsumer
{
    public int received = 0;

    public bool TryAcceptItem(string resourceId)
    {
        received++;
        Debug.Log($"[DummyConsumer] received {resourceId}. Total={received}");
        return true; // zawsze przyjmujemy
    }

    public Vector3 GetIntakePosition() => transform.position;
}
