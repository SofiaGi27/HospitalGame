using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    public Transform target;
    public float distancia = 1f;               // Cerca del personaje
    public float altura = 1.7f;                // Altura tipo cabeza
    public float desplazamientoLateral = 0f;   // Centrada detrás
    public float suavizado = 0.05f;

    private Vector3 velocidadSuavizado = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // Posición deseada muy cerca y centrada detrás de la cabeza
        Vector3 posicionDeseada = target.position
                                  - target.forward * distancia
                                  + target.right * desplazamientoLateral
                                  + Vector3.up * altura;

        // Movimiento suavizado
        transform.position = Vector3.SmoothDamp(transform.position, posicionDeseada, ref velocidadSuavizado, suavizado);

        // Enfoca justo al frente de la cabeza del personaje
        Vector3 puntoDeMira = target.position + Vector3.up * 1.6f + target.forward * 1f;
        transform.LookAt(puntoDeMira);
    }
}
