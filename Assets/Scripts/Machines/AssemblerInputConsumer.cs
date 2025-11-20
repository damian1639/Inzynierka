using UnityEngine;

public class AssemblerInputConsumer : MonoBehaviour, IConveyorConsumer
{
    public enum SlotKind { A, B }

    [Header("Powiązania")]
    public Assembler assembler;
    public SlotKind slot = SlotKind.A;
    public Transform intakePoint;   // opcjonalny punkt wejścia

    // --- IConveyorConsumer ---

    // tu taśma pyta: "gdzie mam celować?"
    public Vector3 GetIntakePosition()
    {
        return intakePoint != null ? intakePoint.position : transform.position;
    }

    // tu taśma próbuje oddać 1 item
    public bool TryAcceptItem(string resourceId)
    {
        if (assembler == null || string.IsNullOrEmpty(resourceId))
            return false;

        bool ok;
        if (slot == SlotKind.A)
            ok = assembler.AcceptFromBeltToSlotA(resourceId);
        else
            ok = assembler.AcceptFromBeltToSlotB(resourceId);

        // do debugowania:
        if (ok)
            Debug.Log($"[AssemblerInputConsumer {slot}] przyjął {resourceId}");
        return ok;
    }

    // kolorowa kulka w edytorze, żebyś widział wejście
    private void OnDrawGizmos()
    {
        Gizmos.color = (slot == SlotKind.A) ? Color.cyan : Color.yellow;
        Vector3 p = intakePoint != null ? intakePoint.position : transform.position;
        Gizmos.DrawSphere(p, 0.1f);
    }
}
