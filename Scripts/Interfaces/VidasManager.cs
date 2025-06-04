using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; // Agregado para manejar escenas

public class VidasManager : MonoBehaviour
{
    // Singleton instance
    public static VidasManager Instance { get; private set; }

    [Header("Configuración Principal")]
    [SerializeField] private int vidasIniciales = 3;
    [SerializeField] private Texture2D heartTexture;
    [SerializeField] private string gameOverSceneName = "GameOverScene"; // Nombre de la escena GameOver
    [SerializeField] private string[] escenasConUI = { "Game", "PreguntaScene" }; // Escenas donde se debe mostrar la UI

    [Header("Referencia UI (Opcional)")]
    [Tooltip("No es necesario asignarlo, el script buscará automáticamente el UIDocument en cada escena")]
    [SerializeField] private string nombreUIDocument = "VidasUI"; // Nombre del GameObject que contiene el UIDocument

    // Referencias a elementos UI
    private UIDocument vidasDocument;
    private VisualElement vidasContainer;
    private VisualElement[] vidasIconos = new VisualElement[3];
    private Label vidasLabel;
    private int vidasActuales;
    private bool uiInicializada = false;

    private void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //Debug.Log("VidasManager Singleton creado y marcado como DontDestroyOnLoad");

            // Cargar el estado de vidas guardado
            CargarEstadoVidas();

            // Suscribirse al evento de cambio de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            //Debug.Log("Se destruye una instancia duplicada de VidasManager");
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
        // Si no se inicializó en OnEnable, intentar de nuevo
        if (!uiInicializada)
        {
            InitializeUI();
            ReiniciarVidas();
        }
    }

    private void InitializeUI()
    {
        // Buscar el UIDocument en la escena actual
        BuscarUIDocumentEnEscena();

        if (vidasDocument == null)
        {
            Debug.LogWarning($"No se ha encontrado ningún UIDocument en la escena {SceneManager.GetActiveScene().name}. " +
                            $"Asegúrate de tener un GameObject con un UIDocument llamado '{nombreUIDocument}' o ajusta el nombre en el Inspector.");
            return;
        }

        var root = vidasDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogWarning("Root VisualElement no disponible. El UIDocument podría no estar correctamente configurado.");
            return;
        }

        vidasContainer = root.Q<VisualElement>("vidas-container");
        vidasLabel = root.Q<Label>("vidas-contador");

        if (vidasContainer == null)
        {
            Debug.LogWarning("No se encontró el elemento 'vidas-container' en el documento UI. Verifica que tu UXML tenga este elemento.");
            return;
        }

        if (vidasLabel == null)
        {
            Debug.LogWarning("No se encontró el elemento 'vidas-contador' en el documento UI");
        }

        for (int i = 0; i < 3; i++)
        {
            vidasIconos[i] = root.Q<VisualElement>($"vida-icono-{i}");

            if (vidasIconos[i] == null)
            {
                //Debug.LogWarning($"No se encontró el elemento 'vida-icono-{i}' en el documento UI");
                continue;
            }

            // Configurar la imagen desde el script si se asignó una textura
            if (heartTexture != null)
            {
                vidasIconos[i].style.backgroundImage = new StyleBackground(heartTexture);
            }
        }

        uiInicializada = true;
        ActualizarVidasVisual(vidasActuales);
        Debug.Log($"VidasManager UI inicializada correctamente en la escena {SceneManager.GetActiveScene().name}");
    }

    // Método para buscar el UIDocument en la escena actual
    private void BuscarUIDocumentEnEscena()
    {
        // Intentar encontrar por nombre específico primero
        GameObject uiObject = GameObject.Find(nombreUIDocument);

        if (uiObject != null)
        {
            vidasDocument = uiObject.GetComponent<UIDocument>();
            if (vidasDocument != null)
            {
                Debug.Log($"UIDocument encontrado por nombre '{nombreUIDocument}'");
                return;
            }
        }

        // Si no se encuentra por nombre, buscar cualquier UIDocument en la escena
        UIDocument[] documentosUI = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);

        if (documentosUI.Length > 0)
        {
            // Usar el primer UIDocument encontrado
            vidasDocument = documentosUI[0];
            //Debug.Log($"UIDocument encontrado en GameObject '{vidasDocument.gameObject.name}'");
            return;
        }

        // Si llegamos aquí, no se encontró ningún UIDocument
        Debug.LogWarning("No se encontró ningún UIDocument en la escena actual");
        vidasDocument = null;
    }

    // Este método será llamado cuando el GameManager notifique un cambio en las vidas
    public void ActualizarVidasVisual(int cantidadVidas)
    {
        // Si no estamos en una escena con UI de vidas, no hacer nada
        if (!EsEscenaConUI())
        {
            //Debug.Log("No actualizando UI de vidas porque no estamos en una escena que la contiene");
            return;
        }

        // Si la UI no está inicializada, intentar inicializarla
        if (!uiInicializada)
        {
            InitializeUI();
            if (!uiInicializada) return; // Si aún no se pudo inicializar, salir
        }

        //Debug.Log($"Actualizando vidas visuales a: {cantidadVidas}");

        // Actualizar el contador de texto
        if (vidasLabel != null)
        {
            vidasLabel.text = "x" + cantidadVidas.ToString();
        }
        else
        {
            //Debug.LogWarning("vidasLabel es null cuando se intenta actualizar");
        }

        // Actualizar la visualización de iconos
        for (int i = 0; i < vidasIconos.Length; i++)
        {
            if (vidasIconos[i] != null)
            {
                bool mostrar = i < cantidadVidas;
                // Asegurarse de que los cambios sean visibles
                vidasIconos[i].style.display = mostrar ? DisplayStyle.Flex : DisplayStyle.None;
                vidasIconos[i].style.visibility = mostrar ? Visibility.Visible : Visibility.Hidden;
                // Registrar cambios para depuración
                //Debug.Log($"Icono de vida {i}: {(mostrar ? "Visible" : "Oculto")}");
            }
            else
            {
                Debug.LogWarning($"vidasIconos[{i}] es null cuando se intenta actualizar");
            }
        }

        // Forzar actualización visual del contenedor
        if (vidasContainer != null)
        {
            vidasContainer.MarkDirtyRepaint();
        }
        else
        {
            Debug.LogWarning("vidasContainer es null cuando se intenta actualizar");
        }
    }

    // Método público para quitar una vida
    public void QuitarVida()
    {
        if (vidasActuales > 0)
        {
            vidasActuales--;
            ActualizarVidasVisual(vidasActuales);

            // Guardar el estado actual de vidas para asegurar la persistencia
            PlayerPrefs.SetInt("VidasActuales", vidasActuales);
            PlayerPrefs.Save();

            //Debug.Log($"Vida quitada. Vidas restantes: {vidasActuales}");
            if (vidasActuales <= 0)
            {
                // Buscar y avisar al QuizManager antes de cambiar de escena
                QuizManager quizManager = FindObjectOfType<QuizManager>();
                if (quizManager != null)
                {
                    quizManager.OnJugadorSinVidas();
                }

                // Pequeño delay para asegurar que se guarde el puntaje
                StartCoroutine(CargarGameOver());

            }
            
        }
    }

    // Corrutina para cargar la escena GameOver con un pequeño retraso
    private IEnumerator CargarGameOver()
    {
        // Pequeña pausa para que el jugador vea que se quedó sin vidas
        yield return new WaitForSeconds(1f);
        Debug.Log("Cargando escena GameOver...");
        SceneManager.LoadScene(gameOverSceneName);
    }

    // Método público para agregar una vida
    public void AgregarVida()
    {
        if (vidasActuales < vidasIconos.Length)
        {
            vidasActuales++;
            ActualizarVidasVisual(vidasActuales);

            // Guardar el estado actual de vidas
            PlayerPrefs.SetInt("VidasActuales", vidasActuales);
            PlayerPrefs.Save();

            Debug.Log($"Vida añadida. Vidas actuales: {vidasActuales}");
        }
    }

    // Método para reiniciar las vidas
    public void ReiniciarVidas()
    {
        vidasActuales = vidasIniciales;
        ActualizarVidasVisual(vidasActuales);

        // Guardar el estado reiniciado
        PlayerPrefs.SetInt("VidasActuales", vidasActuales);
        PlayerPrefs.Save();

        Debug.Log($"Vidas reiniciadas a: {vidasActuales}");
    }

    // Método para cargar el estado de las vidas
    private void CargarEstadoVidas()
    {
        if (PlayerPrefs.HasKey("VidasActuales"))
        {
            vidasActuales = PlayerPrefs.GetInt("VidasActuales");
            //Debug.Log($"Estado de vidas cargado: {vidasActuales}");
        }
        else
        {
            vidasActuales = vidasIniciales;
            Debug.Log($"No se encontró estado guardado. Vidas inicializadas a: {vidasActuales}");
        }

        ActualizarVidasVisual(vidasActuales);
    }

    // Método que se llama cuando se carga una nueva escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Escena cargada: {scene.name} - Verificando UI de vidas");

        // Reiniciar la bandera de UI inicializada
        uiInicializada = false;

        // Si estamos en una escena que debe mostrar la UI, intentar inicializarla después de un breve retraso
        if (EsEscenaConUI())
        {
            StartCoroutine(ReconectarUIConRetraso());
        }
    }

    // Comprueba si la escena actual debe mostrar la UI de vidas
    private bool EsEscenaConUI()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        foreach (string escena in escenasConUI)
        {
            if (escena == escenaActual)
                return true;
        }
        return false;
    }

    private IEnumerator ReconectarUIConRetraso()
    {
        // Dar tiempo a que la nueva escena se cargue completamente
        yield return new WaitForSeconds(0.5f);

        // Reintentar inicializar la UI
        InitializeUI();

        // Actualizar la visualización con el estado actual
        ActualizarVidasVisual(vidasActuales);
    }

    // Este método debe ser llamado desde el GameManager cuando se detecte una respuesta incorrecta
    public void ProcesarRespuestaIncorrecta()
    {
        if (PlayerPrefs.GetInt("RespuestaIncorrecta", 0) == 1)
        {
            QuitarVida();
            PlayerPrefs.SetInt("RespuestaIncorrecta", 0);
            PlayerPrefs.Save();
        }
    }

    // Métodos para sobrescribir OnEnable
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
}