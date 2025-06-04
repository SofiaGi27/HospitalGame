using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TankCharacterController : MonoBehaviour
{
    public float velocidad = 1f;
    public float rotacionVelocidad = 180f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float inputVertical = Input.GetAxis("Vertical");
        float inputHorizontal = Input.GetAxis("Horizontal");

        // Movimiento hacia adelante/atrás
        Vector3 movimiento = transform.forward * inputVertical * velocidad;
        controller.SimpleMove(movimiento);

        // Rotación sobre eje Y (izquierda/derecha)
        transform.Rotate(Vector3.up * inputHorizontal * rotacionVelocidad * Time.deltaTime);
    }
}
