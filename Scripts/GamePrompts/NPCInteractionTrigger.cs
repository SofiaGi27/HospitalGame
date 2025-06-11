using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCInteractionTrigger : MonoBehaviour
{
    [SerializeField] private RoomMessageController messageController;
    [SerializeField][TextArea(3, 5)] private string interactionMessage = "Presiona E para responder las preguntas del médico.";
    [SerializeField] private string sceneToLoad = "PreguntaScene";
    [SerializeField] private int especialidad=1;

    private bool playerInRange = false;
    private bool messageShown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (!messageShown && messageController != null)
            {
                messageController.ShowMessage(interactionMessage);
                messageShown = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            messageShown = false; // Para que el mensaje vuelva a aparecer al regresar
        }
    }



    private static bool algunaEspecialidadYaSeleccionada = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !algunaEspecialidadYaSeleccionada)
        {
            algunaEspecialidadYaSeleccionada = true;

            UserSession.Instance.SetEspecialidadActual(especialidad);
            StartCoroutine(CargarEscenaConDelay());
        }
    }
    private IEnumerator CargarEscenaConDelay()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneToLoad);
    }
}
