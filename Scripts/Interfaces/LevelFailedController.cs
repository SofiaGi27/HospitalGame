using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class LevelFailedController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument;

    private VisualElement root;
    private VisualElement mainContainer;
    private Label titleText;
    private Button continueButton;
    private VisualElement sadFace;
    private VisualElement leftCloud;
    private VisualElement rightCloud;

     // Sonido
    public AudioClip ambientMusicClip;
    public AudioClip buttonClickClip;

    private AudioSource ambientAudioSource;
    private AudioSource sfxAudioSource;

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        // Crear fuentes de audio si no existen
        ambientAudioSource = gameObject.AddComponent<AudioSource>();
        ambientAudioSource.loop = false;
        ambientAudioSource.playOnAwake = false;

        sfxAudioSource = gameObject.AddComponent<AudioSource>();
        sfxAudioSource.playOnAwake = false;

    }

   private void OnEnable()
    {
        InitializeUI();

        if (ambientMusicClip != null && ambientAudioSource != null)
        {
            ambientAudioSource.clip = ambientMusicClip;
            ambientAudioSource.Play();
        }
    }
    private void Start()
    {
        int especialidadId = PlayerPrefs.GetInt("EspecialidadCompletada", 1);
        int puntajeFinal = PlayerPrefs.GetInt("PuntajeFinal", 0);
        bool esVictoria = PlayerPrefs.GetInt("EsVictoria", 0) == 1;

        // Usar la especialidad para mostrar el texto correcto
        SetupLevelFailed(especialidadId);

        // Limpiar los PlayerPrefs despu�s de usarlos
        PlayerPrefs.DeleteKey("EspecialidadCompletada");
        PlayerPrefs.DeleteKey("PuntajeFinal");
        PlayerPrefs.DeleteKey("EsVictoria");
    }
    private void InitializeUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument no est� asignado en LevelFailedController");
            return;
        }

        root = uiDocument.rootVisualElement;

        // Obtener referencias a los elementos UI por nombre
        mainContainer = root.Q<VisualElement>("MainContainer");
        titleText = root.Q<Label>("Title");
        continueButton = root.Q<Button>("ContinueButton");
        sadFace = root.Q<VisualElement>("SadFace");
        leftCloud = root.Q<VisualElement>("LeftCloud");
        rightCloud = root.Q<VisualElement>("RightCloud");

        // Si no encuentra por nombre, intentar por clase como respaldo
        if (titleText == null)
            titleText = root.Q<Label>(className: "title-text");
        if (continueButton == null)
            continueButton = root.Q<Button>(className: "continue-button");

        // Configurar eventos
        if (continueButton != null)
        {
            continueButton.clicked += OnContinueButtonClicked;
        }
        else
        {
            Debug.LogWarning("No se pudo encontrar el bot�n continuar en LevelFailedController");
        }

        // Verificar que se encontraron los elementos esenciales
        if (titleText == null)
        {
            Debug.LogWarning("No se pudo encontrar el elemento Title en la UI de Level Failed");
        }
    }

    /// <summary>
    /// Configura la pantalla de nivel fallido para una especialidad espec�fica
    /// </summary>
    /// <param name="especialidadId">ID de la especialidad no completada</param>
    public void SetupLevelFailed(int especialidadId)
    {
        if (MySQLManager.Instance?.especialidadService == null)
        {
            Debug.LogError("MySQLManager o EspecialidadService no est�n disponibles");
            return;
        }

        try
        {
            // Obtener el nombre de la especialidad
            string especialidadName = MySQLManager.Instance.especialidadService.GetEspecialidadName(especialidadId);

            if (string.IsNullOrEmpty(especialidadName))
            {
                Debug.LogWarning($"No se pudo obtener el nombre de la especialidad con ID: {especialidadId}");
                especialidadName = "ESPECIALIDAD DESCONOCIDA";
            }

            // Actualizar el texto del t�tulo
            UpdateTitleText(especialidadName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al obtener el nombre de la especialidad: {ex.Message}");
            UpdateTitleText("ESPECIALIDAD");
        }
    }

    /// <summary>
    /// Actualiza el texto del t�tulo con el nombre de la especialidad
    /// </summary>
    /// <param name="especialidadName">Nombre de la especialidad</param>
    private void UpdateTitleText(string especialidadName)
    {
        if (titleText != null)
        {
            string failedText = $"�NO HAS COMPLETADO LA ESPECIALIDAD DE {especialidadName.ToUpper()}!";
            titleText.text = failedText;
        }
        else
        {
            Debug.LogWarning("No se pudo encontrar el elemento Title en la UI");
        }
    }

    /// <summary>
    /// Maneja el clic del bot�n continuar
    /// </summary>
    private void OnContinueButtonClicked()
    {
        Debug.Log("Bot�n continuar presionado en pantalla de nivel fallido");
        if (buttonClickClip != null && sfxAudioSource != null)
        {
            sfxAudioSource.PlayOneShot(buttonClickClip);
        }

        OnContinue();
    }

    /// <summary>
    /// M�todo virtual que puede ser sobrescrito para personalizar la acci�n de continuar
    /// </summary>
    protected virtual void OnContinue()
    {
        SceneManager.LoadScene("Game");

    }

    /// <summary>
    /// Evento que se dispara cuando se completa la interacci�n con la pantalla de nivel fallido
    /// </summary>
    public System.Action OnLevelFailedComplete;

    /// <summary>
    /// Evento que se dispara cuando el usuario quiere reintentar el nivel
    /// </summary>
    public System.Action OnRetryLevel;

    /// <summary>
    /// Evento que se dispara cuando el usuario quiere volver al men� principal
    /// </summary>
    public System.Action OnBackToMenu;

    private void OnDisable()
    {
        // Limpiar eventos para evitar memory leaks
        if (continueButton != null)
        {
            continueButton.clicked -= OnContinueButtonClicked;
        }
    }

    #region M�todos p�blicos para uso externo

    /// <summary>
    /// Muestra la pantalla de nivel fallido para una especialidad espec�fica
    /// </summary>
    /// <param name="especialidadId">ID de la especialidad no completada</param>
    public void ShowLevelFailed(int especialidadId)
    {
        SetupLevelFailed(especialidadId);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Oculta la pantalla de nivel fallido
    /// </summary>
    public void HideLevelFailed()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Configura el texto del t�tulo manualmente 
    /// </summary>
    /// <param name="customText">Texto personalizado para mostrar</param>
    public void SetCustomTitleText(string customText)
    {
        if (titleText != null)
        {
            titleText.text = customText;
        }
    }

    /// <summary>
    /// Obtiene referencia al bot�n continuar para configuraciones adicionales
    /// </summary>
    /// <returns>Referencia al bot�n continuar</returns>
    public Button GetContinueButton()
    {
        return continueButton;
    }

    /// <summary>
    /// Obtiene referencia al elemento del t�tulo para configuraciones adicionales
    /// </summary>
    /// <returns>Referencia al label del t�tulo</returns>
    public Label GetTitleLabel()
    {
        return titleText;
    }

    #endregion


}