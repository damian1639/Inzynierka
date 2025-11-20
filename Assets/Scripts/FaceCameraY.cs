using UnityEngine;

[ExecuteAlways]
public class FaceCameraY : MonoBehaviour
{
    [Header("Docelowa kamera (zostaw puste = Main Camera)")]
    public Camera targetCamera;

    [Header("Ustawienia obrotu")]
    public bool lockYOnly = true;   // obrót tylko w poziomie
    public float extraYaw = 0f;     // np. 180 gdy model jest odwrócony

    void OnEnable()
    {
        EnsureCamera();
        ApplyRotation(); // wyrównaj od razu, bez czekania na Update
    }

    void Update()
    {
        // W Edit Mode Update wywołuje się rzadziej niż w Play, ale to wystarczy,
        // żeby zachować spójny wygląd w oknie Game.
        EnsureCamera();
        ApplyRotation();
    }

    void EnsureCamera()
    {
        if (targetCamera != null) return;

        // Najpierw spróbuj Main Camera po tagu
        var main = Camera.main;
        if (main != null) { targetCamera = main; return; }

        // Jeśli projekt nie ma jeszcze taga MainCamera – weź pierwszą kamerę ze sceny
        var anyCam = Object.FindObjectOfType<Camera>();
        if (anyCam != null) targetCamera = anyCam;
    }

    void ApplyRotation()
    {
        if (targetCamera == null) return;

        Vector3 fwd = targetCamera.transform.forward;

        if (lockYOnly)
        {
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 1e-6f) return;
            fwd.Normalize();
        }

        transform.rotation = Quaternion.LookRotation(fwd) * Quaternion.Euler(0f, extraYaw, 0f);
    }

    // Dzięki temu po zmianie wartości w Inspectorze od razu zobaczysz efekt
    void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        EnsureCamera();
        ApplyRotation();
    }
}
