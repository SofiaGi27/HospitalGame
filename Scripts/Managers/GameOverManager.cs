using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private string sceneToLoad = "Game"; 
    [SerializeField] private UIDocument uiDocument; 

    private Button retryButton; // Botón para volver a jugar

    private void Awake()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("No se encontró el componente UIDocument.");
                return;
            }
        }
    }

    private void OnEnable()
    {
        // Obtener la referencia al botón cuando el UIDocument está habilitado
        var root = uiDocument.rootVisualElement;
        retryButton = root.Q<Button>("RetryButton");

        if (retryButton != null)
        {
            // Asignar el evento click al botón
            retryButton.clicked += RetryGame;
        }
        else
        {
            Debug.LogError("No se encontró el botón 'RetryButton'. Verifica el nombre en el UI Builder.");
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

    // Método para reiniciar el juego
    public void RetryGame()
    {
        // Cargar la escena principal del juego
        SceneManager.LoadScene(sceneToLoad);
    }
}