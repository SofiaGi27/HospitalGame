using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; // Agregado para manejar escenas

public class VidasManager : MonoBehaviour
{
    // Singleton instance
    public static VidasManager Instance { get; private set; }

    [SerializeField] private UIDocument vidasDocument;
    [SerializeField] private int vidasIniciales = 3;
    [SerializeField] private Texture2D heartTexture;

    // Referencias a elementos UI
    private VisualElement vidasContainer;
    private VisualElement[] vidasIconos = new VisualElement[3];
    private Label vidasLabel;
    private int vidasActuales;
    private bool uiInicializada = false;

    // Referencia al QuizManager
    private QuizManager quizManager;

    private void Awake()
    {

        // Implementaci�n del patr�n Singleton
        if (Instance == null)
        {
            Instance = this;

            LimpiarVidasGuardadas(); // Solo en el editor para testing

            // Cargar el estado de vidas guardado
            CargarEstadoVidas();

            // Suscribirse al evento de cambio de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Debug.Log("Se destruye una instancia duplicada de VidasManager");
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        if (Instance == this && !uiInicializada)
        {
            // Inicializar UI
            InitializeUI();
        }
    }

    private void Start()
    {
        // Si no se inicializ� en OnEnable, intentar de nuevo
        if (!uiInicializada)
        {
            InitializeUI();
        }

        // Buscar referencia al QuizManager
        BuscarQuizManager();
    }

    // M�todo para buscar y establecer la referencia al QuizManager
    private void BuscarQuizManager()
    {
        // Usar la instancia Singleton del QuizManager
        quizManager = QuizManager.Instance;
        if (quizManager != null)
        {
            Debug.Log("QuizManager Singleton encontrado y referenciado");
        }
        else
        {
            Debug.LogWarning("QuizManager Singleton no encontrado");
        }
    }

    private void InitializeUI()
    {
        if (vidasDocument == null)
        {
            vidasDocument = GetComponent<UIDocument>();
            if (vidasDocument == null)
            {
                Debug.LogWarning("No se ha encontrado el UIDocument en VidasManager. Esto es normal si estamos en una escena sin UI de vidas.");
                return;
            }
        }

        var root = vidasDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogWarning("Root VisualElement no disponible. Posiblemente estamos en una escena sin UI de vidas.");
            return;
        }

        vidasContainer = root.Q<VisualElement>("vidas-container");
        vidasLabel = root.Q<Label>("vidas-contador");

        if (vidasContainer == null)
        {
            Debug.LogWarning("No se encontr� el elemento 'vidas-container' en el documento UI. Posiblemente estamos en una escena sin UI de vidas.");
            return;
        }

        if (vidasLabel == null)
        {
            Debug.LogWarning("No se encontr� el elemento 'vidas-contador' en el documento UI");
        }

        for (int i = 0; i < 3; i++)
        {
            vidasIconos[i] = root.Q<VisualElement>($"vida-icono-{i}");

            if (vidasIconos[i] == null)
            {
                Debug.LogWarning($"No se encontr� el elemento 'vida-icono-{i}' en el documento UI");
                continue;
            }

            // Configurar la imagen desde el script si se asign� una textura
            if (heartTexture != null)
            {
                vidasIconos[i].style.backgroundImage = new StyleBackground(heartTexture);
            }
        }

        uiInicializada = true;
        ActualizarVidasVisual(vidasActuales);
        Debug.Log("VidasManager UI inicializada correctamente");
    }

    // Este m�todo ser� llamado cuando el GameManager notifique un cambio en las vidas
    public void ActualizarVidasVisual(int cantidadVidas)
    {
        // Si la UI no est� inicializada, intentar inicializarla
        if (!uiInicializada)
        {
            InitializeUI();
            if (!uiInicializada) return; // Si a�n no se pudo inicializar, salir
        }

        Debug.Log($"Actualizando vidas visuales a: {cantidadVidas}");

        // Actualizar el contador de texto
        if (vidasLabel != null)
        {
            vidasLabel.text = "x" + cantidadVidas.ToString();
        }
        else
        {
            Debug.LogWarning("vidasLabel es null cuando se intenta actualizar");
        }

        // Actualizar la visualizaci�n de iconos
        for (int i = 0; i < vidasIconos.Length; i++)
        {
            if (vidasIconos[i] != null)
            {
                bool mostrar = i < cantidadVidas;
                // Asegurarse de que los cambios sean visibles
                vidasIconos[i].style.display = mostrar ? DisplayStyle.Flex : DisplayStyle.None;
                vidasIconos[i].style.visibility = mostrar ? Visibility.Visible : Visibility.Hidden;
                // Registrar cambios para depuraci�n
                Debug.Log($"Icono de vida {i}: {(mostrar ? "Visible" : "Oculto")}");
            }
            else
            {
                Debug.LogWarning($"vidasIconos[{i}] es null cuando se intenta actualizar");
            }
        }

        // Forzar actualizaci�n visual del contenedor
        if (vidasContainer != null)
        {
            vidasContainer.MarkDirtyRepaint();
        }
        else
        {
            Debug.LogWarning("vidasContainer es null cuando se intenta actualizar");
        }
    }

    // M�todo p�blico para quitar una vida
    public void QuitarVida()
    {
        if (vidasActuales > 0)
        {
            vidasActuales--;
            ActualizarVidasVisual(vidasActuales);

            // Guardar el estado actual de vidas para asegurar la persistencia
            PlayerPrefs.SetInt("VidasActuales", vidasActuales);
            PlayerPrefs.Save();

            Debug.Log($"Vida quitada. Vidas restantes: {vidasActuales}");

            // Si ya no quedan vidas, llamar al m�todo del QuizManager
            if (vidasActuales <= 0)
            {
                StartCoroutine(NotificarJugadorSinVidas());
            }
        }
    }

    // Corrutina para notificar al QuizManager que el jugador se qued� sin vidas
    private IEnumerator NotificarJugadorSinVidas()
    {
        // Peque�a pausa para que el jugador vea que se qued� sin vidas
        yield return new WaitForSeconds(1f);

        // Buscar QuizManager si no est� referenciado
        if (quizManager == null)
        {
            BuscarQuizManager();
        }

        // Llamar al m�todo del QuizManager si est� disponible
        if (quizManager != null)
        {
            Debug.Log("Llamando a OnJugadorSinVidas() del QuizManager...");
            quizManager.OnJugadorSinVidas();
        }
        else
        {
            Debug.LogError("No se pudo encontrar el QuizManager Singleton para notificar que el jugador se qued� sin vidas");
            // Fallback: cargar la escena GameOver directamente
            Debug.Log("Fallback: Cargando escena GameOver directamente...");
            SceneManager.LoadScene(5);
        }
    }

    // M�todo p�blico para agregar una vida
    public void AgregarVida()
    {
        if (vidasActuales < vidasIconos.Length)
        {
            vidasActuales++;
            ActualizarVidasVisual(vidasActuales);

            // Guardar el estado actual de vidas
            PlayerPrefs.SetInt("VidasActuales", vidasActuales);
            PlayerPrefs.Save();

            Debug.Log($"Vida a�adida. Vidas actuales: {vidasActuales}");
        }
    }

    // M�todo para reiniciar las vidas
    public void ReiniciarVidas()
    {
        vidasActuales = vidasIniciales;
        ActualizarVidasVisual(vidasActuales);

        // Guardar el estado reiniciado
        PlayerPrefs.SetInt("VidasActuales", vidasActuales);
        PlayerPrefs.Save();

        Debug.Log($"Vidas reiniciadas a: {vidasActuales}");
    }

    // M�todo para cargar el estado de las vidas
    private void CargarEstadoVidas()
    {
        if (PlayerPrefs.HasKey("VidasActuales"))
        {
            vidasActuales = PlayerPrefs.GetInt("VidasActuales");
            Debug.Log($"Estado de vidas cargado: {vidasActuales}");
        }
        else
        {
            vidasActuales = vidasIniciales;
            Debug.Log($"No se encontr� estado guardado. Vidas inicializadas a: {vidasActuales}");
        }

        ActualizarVidasVisual(vidasActuales);
    }

    // M�todo que se llama cuando se carga una nueva escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Escena cargada: {scene.name} - Verificando UI de vidas");

        // Reiniciar la bandera de UI inicializada
        uiInicializada = false;

        // Buscar nuevamente el QuizManager en la nueva escena
        BuscarQuizManager();
    }

    private IEnumerator ReconectarUIConRetraso()
    {
        // Dar tiempo a que la nueva escena se cargue completamente
        yield return new WaitForSeconds(0.5f);

        // Reintentar inicializar la UI
        InitializeUI();

        // Actualizar la visualizaci�n con el estado actual
        ActualizarVidasVisual(vidasActuales);
    }

    // Este m�todo debe ser llamado desde el GameManager cuando se detecte una respuesta incorrecta
    public void ProcesarRespuestaIncorrecta()
    {
        if (PlayerPrefs.GetInt("RespuestaIncorrecta", 0) == 1)
        {
            QuitarVida();
            PlayerPrefs.SetInt("RespuestaIncorrecta", 0);
            PlayerPrefs.Save();
        }
    }

    // M�todos para sobrescribir OnEnable
    private void OnDisable()
    {
        // Guardar el estado actual al desactivar
        if (Instance == this)
        {
            PlayerPrefs.SetInt("VidasActuales", vidasActuales);
            PlayerPrefs.Save();
        }
    }

    // Desuscribirse del evento al destruir el objeto
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // M�todo para limpiar las vidas guardadas (�til para testing)
    private void LimpiarVidasGuardadas()
    {
        if (PlayerPrefs.HasKey("VidasActuales"))
        {
            PlayerPrefs.DeleteKey("VidasActuales");
            PlayerPrefs.Save();
            Debug.Log("Vidas guardadas limpiadas");
        }
    }

    // M�todo p�blico para obtener las vidas actuales
    public int GetVidasActuales()
    {
        return vidasActuales;
    }
}