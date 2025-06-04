using UnityEngine;

public class MessageZoneTrigger : MonoBehaviour
{
    [SerializeField] private RoomMessageController messageController;

    [TextArea(3, 5)]
    [SerializeField] private string messageText = "Mensaje por defecto...";

    [SerializeField] private float cooldownDuration = 10f;

    private float lastTriggerTime = -Mathf.Infinity;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Time.time >= lastTriggerTime + cooldownDuration)
        {
            messageController.ShowMessage(messageText);
            lastTriggerTime = Time.time;
        }
    }
}
