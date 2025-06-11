using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target;

    public float distancia = 3.0f;
    public float altura = 2.0f;
    public float desplazamientoLateral = 0f;
    public float suavizado = 0.2f;

    private Vector3 velocidadSuavizado = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // Posición deseada de la cámara en base a la posición del personaje
        Vector3 posicionDeseada = target.position
                                  - target.forward * distancia
                                  + target.right * desplazamientoLateral
                                  + Vector3.up * altura;

        // Suaviza la transición hacia la posición deseada
        transform.position = Vector3.SmoothDamp(transform.position, posicionDeseada, ref velocidadSuavizado, suavizado);

        // Hacer que la cámara mire al personaje (ajustado al torso)
        Vector3 puntoDeMira = target.position + Vector3.up * 1.5f;
        transform.LookAt(puntoDeMira);
    }
}
