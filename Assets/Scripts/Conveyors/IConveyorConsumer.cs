public interface IConveyorConsumer
{
    // Spróbuj przyjąć JEDNĄ sztukę danego surowca.
    // Zwraca true, jeśli przyjęto (czyli Miner może zmniejszyć swój bufor).
    bool TryAcceptItem(string resourceId);

    // Punkt w świecie, gdzie obiekt chce odbierać itemy (nieobowiązkowe, ale przydatne).
    UnityEngine.Vector3 GetIntakePosition();
}
