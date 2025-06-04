using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Game"; 
    [SerializeField] private UIDocument uiDocument; 

    private Button retryButton; // Bot�n para volver a jugar

    private void Awake()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("No se encontr� el componente UIDocument.");
                return;
            }
        }
    }

    private void OnEnable()
    {
        // Obtener la referencia al bot�n cuando el UIDocument est� habilitado
        var root = uiDocument.rootVisualElement;
        retryButton = root.Q<Button>("RetryButton");

        if (retryButton != null)
        {
            // Asignar el evento click al bot�n
            retryButton.clicked += RetryGame;
        }
        else
        {
            Debug.LogError("No se encontr� el bot�n 'RetryButton'. Verifica el nombre en el UI Builder.");
        }
    }

    private void OnDisable()
    {
        // Quitar el evento al deshabilitar para evitar memory leaks
        if (retryButton != null)
        {
            retryButton.clicked -= RetryGame;
        }
    }

    // M�todo para reiniciar el juego
    public void RetryGame()
    {
        // Cargar la escena principal del juego
        SceneManager.LoadScene(sceneToLoad);
    }
}