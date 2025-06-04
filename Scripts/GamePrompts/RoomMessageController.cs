using UnityEngine;
using UnityEngine.UIElements;
using System; 

public class RoomMessageController : MonoBehaviour
{
    [TextArea(3, 5)]
    public string customMessage = "Te damos la bienvenida a HUPARCHIS. Hoy comienza tu recorrido por nuestro hospital...";

    [SerializeField] private float displayDuration = 8f; // Duración más larga

    private VisualElement messagePanel;
    private Label messageText;

    private void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        messagePanel = root.Q<VisualElement>("message-panel");
        messageText = root.Q<Label>("message-text");

        // Ocultamos el panel al principio
        messagePanel.AddToClassList("hidden");
    }

    public void ShowMessage(string message = null)
    {
        if (messageText == null || messagePanel == null) return;

        messageText.text = message ?? customMessage;
        messagePanel.RemoveFromClassList("hidden");

        CancelInvoke(); // Evita duplicados
        Invoke(nameof(HideMessage), displayDuration);
    }

    public void HideMessage()
    {
        if (messagePanel != null)
            messagePanel.AddToClassList("hidden");
    }
}
