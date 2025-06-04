using UnityEngine;

public class WelcomeTrigger : MonoBehaviour
{
    [SerializeField] private RoomMessageController messageController;

    private bool messageShown = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!messageShown && other.CompareTag("Player"))
        {
            messageController.ShowMessage(); // Usa el mensaje por defecto
            messageShown = true;
        }
    }
}
