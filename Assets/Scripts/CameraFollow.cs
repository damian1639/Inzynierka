using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // obiekt gracza (Player)
    public Vector3 offset = new Vector3(0, 10, -10); // stała pozycja kamery względem gracza
    public float smoothSpeed = 5f;  // jak płynnie kamera się porusza

    void LateUpdate()
    {
        if (target == null) return;

        // Docelowa pozycja kamery
        Vector3 desiredPosition = target.position + offset;

        // Płynne przejście
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Ustaw nową pozycję
        transform.position = smoothedPosition;

        // UWAGA: NIE używamy LookAt, kamera jest statycznie ustawiona
        // Jeśli chcesz zmienić kąt kamery, zrób to w Inspectorze (Rotation)
    }
}
