using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class CharacterMovementV2 : MonoBehaviour
{
    public float speed = 3f;
    public float rotationSpeed = 80,f;
    public float gravity = 8.81f;

    private CharacterController controller;
    private Animator animator;
    private Vector3 verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (controller != null)
        {
            // Ajusta el centro del CharacterController (para que colisione bien)
            controller.center = new Vector3(0, controller.height / 2f, 0);
        }
    }

    void Update()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        // Movimiento hacia adelante/atrás
        Vector3 move = transform.forward * v * speed;

        // Rotación izquierda/derecha (tanque)
        transform.Rotate(Vector3.up, h * rotationSpeed * Time.deltaTime);

        // Gravedad
        if (controller.isGrounded)
        {
            verticalVelocity.y = -0.5f;
        }
        else
        {
            // Combina movimiento horizontal con el vertical (gravedad)
            verticalVelocity.y -= gravity * Time.deltaTime;
        }

        // Movimiento final
        Vector3 finalMovement = new Vector3(move.x, verticalVelocity.y, move.z);
        controller.Move(finalMovement * Time.deltaTime);

        // Animaciones
        if (animator != null)
        {
            // Suaviza el cambio de velocidad para que la animación no sea brusca
            animator.SetFloat("Speed", Mathf.Abs(v), 0.1f, Time.deltaTime);
        }
    }
}
