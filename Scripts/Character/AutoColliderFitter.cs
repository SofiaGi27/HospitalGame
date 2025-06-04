using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class AutoColliderFitter : MonoBehaviour
{
    CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        FitController();        
        AlignWithGround();     
    }

    void FitController()
    {
        // Medidas fijas 
        controller.center = new Vector3(0, 0.799937f, 0);
        controller.height = 1.511371f;
        controller.radius = 0.5f;

        Debug.Log($"[AutoColliderFitter] Ajuste manual - Height: {controller.height}, Radius: {controller.radius}, Center: {controller.center}");
    }

    void AlignWithGround()
    {
        // Raycast desde arriba del personaje hacia abajo para encontrar el suelo
        Vector3 rayStart = transform.position + Vector3.up * 5f;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 20f))
        {
            float groundY = hit.point.y;
            float desiredY = groundY + controller.center.y - 0.01f; // Ajuste fino para evitar flotar
            transform.position = new Vector3(transform.position.x, desiredY, transform.position.z);

            Debug.Log($"[AutoColliderFitter] Piso detectado en: {groundY}, personaje colocado en: {desiredY}");
        }
        else
        {
            Debug.LogWarning("[AutoColliderFitter] No se detectó el suelo con el Raycast.");
        }
    }
}
