using UnityEngine;

public class SimpleCharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // Movimiento básico con teclas WASD o flechas
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(moveX, 0, moveZ);

        // Mover el personaje
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);

        // Rotar hacia la dirección de movimiento si se está moviendo
        if (move != Vector3.zero)
        {
            transform.forward = move;
        }
    }
}
