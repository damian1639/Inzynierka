using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float forwardSpeed = 5f; // Prędkość poruszania się do przodu i do tyłu
    public float strafeSpeed = 4f;  // Prędkość poruszania się na boki

    [Header("References")]
    public Transform cameraTransform;

    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Blokuje obracanie się postaci
    }

    void Update()
    {
        // Odczytujemy wciśnięte klawisze (zwracają wartość od -1 do 1)
        float moveX = Input.GetAxis("Horizontal"); // A / D
        float moveZ = Input.GetAxis("Vertical");   // W / S

        // Pobieramy kierunki z kamery i "spłaszczamy" je (ignorujemy oś Y)
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        // Obliczamy osobno wektor ruchu do przodu i na boki, uwzględniając osobne prędkości
        Vector3 forwardMovement = forward * moveZ * forwardSpeed;
        Vector3 strafeMovement = right * moveX * strafeSpeed;
        
        // Łączymy oba wektory, aby uzyskać ostateczny kierunek i prędkość ruchu
        Vector3 finalMovement = forwardMovement + strafeMovement;

        // Poruszamy graczem za pomocą fizyki
        // Time.deltaTime sprawia, że ruch jest płynny i niezależny od klatek na sekundę
        rb.MovePosition(rb.position + finalMovement * Time.deltaTime);
    }
}