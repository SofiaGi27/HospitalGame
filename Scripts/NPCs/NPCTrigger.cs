using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCTrigger : MonoBehaviour
{
    private bool triggered = false;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!triggered && hit.gameObject.CompareTag("NPC"))
        {
            triggered = true;
            Debug.Log("Colisión con NPC detectada, cargando escena...");
            SceneManager.LoadScene("PreguntaScene");
        }
    }
}
