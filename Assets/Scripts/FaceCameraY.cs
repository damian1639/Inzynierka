using UnityEngine;

public class FaceCameraY : MonoBehaviour
{
    public bool lockYOnly = true;   // zostaw true, obrót tylko wokół osi Y
    public float extraYaw = 0f;     // ewentualna poprawka w stopniach (np. 180)

    void LateUpdate()
    {
        var cam = Camera.main;
        if (!cam) return;

        Vector3 fwd = cam.transform.forward;
        if (lockYOnly)
        {
            fwd.y = 0f;                 // rzut na płaszczyznę XZ
            if (fwd.sqrMagnitude < 0.0001f) return;
            fwd.Normalize();
            transform.rotation = Quaternion.LookRotation(fwd) * Quaternion.Euler(0, extraYaw, 0);
        }
        else
        {
            // pełny billboard 3D
            transform.rotation = Quaternion.LookRotation(fwd) * Quaternion.Euler(0, extraYaw, 0);
        }
    }
}
