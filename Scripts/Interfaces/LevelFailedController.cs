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

    private void Awake()
    {
        // Si no se asignó el UIDocument en el inspector, intentar obtenerlo del GameObject
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        InitializeUI();
    }
    private void Start()
    {
        int especialidadId = PlayerPrefs.GetInt("EspecialidadCompletada", 1);
        int puntajeFinal = PlayerPrefs.GetInt("PuntajeFinal", 0);
        bool esVictoria = PlayerPrefs.GetInt("EsVictoria", 0) == 1;

        // Usar la especialidad para mostrar el texto correcto
        SetupLevelFailed(especialidadId);

        // Limpiar los PlayerPrefs después de usarlos
        PlayerPrefs.DeleteKey("EspecialidadCompletada");
        PlayerPrefs.DeleteKey("PuntajeFinal");
        PlayerPrefs.DeleteKey("EsVictoria");
    }
    private void InitializeUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument no está asignado en LevelFailedController");
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
            Debug.LogWarning("No se pudo encontrar el botón continuar en LevelFailedController");
        }

        // Verificar que se encontraron los elementos esenciales
        if (titleText == null)
        {
            Debug.LogWarning("No se pudo encontrar el elemento Title en la UI de Level Failed");
        }
    }

    /// <summary>
    /// Configura la pantalla de nivel fallido para una especialidad específica
    /// </summary>
    /// <param name="especialidadId">ID de la especialidad no completada</param>
    public void SetupLevelFailed(int especialidadId)
    {
        if (MySQLManager.Instance?.especialidadService == null)
        {
            Debug.LogError("MySQLManager o EspecialidadService no están disponibles");
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

            // Actualizar el texto del título
            UpdateTitleText(especialidadName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al obtener el nombre de la especialidad: {ex.Message}");
            UpdateTitleText("ESPECIALIDAD");
        }
    }

    /// <summary>
    /// Actualiza el texto del título con el nombre de la especialidad
    /// </summary>
    /// <param name="especialidadName">Nombre de la especialidad</param>
    private void UpdateTitleText(string especialidadName)
    {
        if (titleText != null)
        {
            string failedText = $"¡NO HAS COMPLETADO LA ESPECIALIDAD DE {especialidadName.ToUpper()}!";
            titleText.text = failedText;
        }
        else
        {
            Debug.LogWarning("No se pudo encontrar el elemento Title en la UI");
        }
    }

    /// <summary>
    /// Maneja el clic del botón continuar
    /// </summary>
    private void OnContinueButtonClicked()
    {
        Debug.Log("Botón continuar presionado en pantalla de nivel fallido");

        OnContinue();
    }

    /// <summary>
    /// Método virtual que puede ser sobrescrito para personalizar la acción de continuar
    /// </summary>
    protected virtual void OnContinue()
    {
        SceneManager.LoadScene("Game");

    }

    /// <summary>
    /// Evento que se dispara cuando se completa la interacción con la pantalla de nivel fallido
    /// </summary>
    public System.Action OnLevelFailedComplete;

    /// <summary>
    /// Evento que se dispara cuando el usuario quiere reintentar el nivel
    /// </summary>
    public System.Action OnRetryLevel;

    /// <summary>
    /// Evento que se dispara cuando el usuario quiere volver al menú principal
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

    #region Métodos públicos para uso externo

    /// <summary>
    /// Muestra la pantalla de nivel fallido para una especialidad específica
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
    /// Configura el texto del título manualmente 
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
    /// Obtiene referencia al botón continuar para configuraciones adicionales
    /// </summary>
    /// <returns>Referencia al botón continuar</returns>
    public Button GetContinueButton()
    {
        return continueButton;
    }

    /// <summary>
    /// Obtiene referencia al elemento del título para configuraciones adicionales
    /// </summary>
    /// <returns>Referencia al label del título</returns>
    public Label GetTitleLabel()
    {
        return titleText;
    }

    #endregion


}