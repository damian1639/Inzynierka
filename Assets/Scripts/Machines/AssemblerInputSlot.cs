using UnityEngine;
using UnityEngine.EventSystems;

public class AssemblerInputSlot : MonoBehaviour, IPointerClickHandler
{
    public enum SlotKind { A, B }

    public AssemblerUI assemblerUI;
    public SlotKind kind = SlotKind.A;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (assemblerUI == null) return;

        if (kind == SlotKind.A)
            assemblerUI.TryInsertFromSelectedA();
        else
            assemblerUI.TryInsertFromSelectedB();
    }
}
