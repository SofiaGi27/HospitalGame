using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class CongratulationsScreenController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("Rewards")]
    private int coinsReward = 0;
    private int targetReward;

    private VisualElement root;
    private Label titleText;
    private Label coinRewardText;
    private Label targetRewardText;
    private Button continueButton;

    private void Awake()
    {
        // Si no se asignó el UIDocument en el inspector, intentar obtenerlo del GameObject
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
    }
    private void Start()
    {
        int especialidadId = PlayerPrefs.GetInt("EspecialidadCompletada", 1);
        int puntajeFinal = PlayerPrefs.GetInt("PuntajeFinal", 0);
        bool esVictoria = PlayerPrefs.GetInt("EsVictoria", 0) == 1;

        targetReward=puntajeFinal;
        UpdateRewardTexts();
        // Usar la especialidad para mostrar el texto correcto
        SetupCongratulations(especialidadId); 

        // Limpiar los PlayerPrefs después de usarlos
        PlayerPrefs.DeleteKey("EspecialidadCompletada");
        PlayerPrefs.DeleteKey("PuntajeFinal");
        PlayerPrefs.DeleteKey("EsVictoria");
    }
    private void OnEnable()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocument no está asignado en CongratulationsScreenController");
            return;
        }

        root = uiDocument.rootVisualElement;

        // Obtener referencias a los elementos UI
        titleText = root.Q<Label>("title-text");
        if (titleText == null)
        {
            // Si no encuentra por nombre, buscar por clase
            titleText = root.Q<Label>(className: "title-text");
        }

        // Obtener las etiquetas de recompensas
        var rewardTexts = root.Query<Label>(className: "reward-text").ToList();
        if (rewardTexts.Count >= 2)
        {
            coinRewardText = rewardTexts[0];
            targetRewardText = rewardTexts[1];
        }

        // Obtener el botón de continuar
        continueButton = root.Q<Button>(className: "continue-button");

        // Configurar eventos
        if (continueButton != null)
        {
            continueButton.clicked += OnContinueButtonClicked;
        }

        // Actualizar valores de recompensas
        UpdateRewardTexts();
    }

    /// <summary>
    /// Configura la pantalla de felicitaciones para una especialidad específica
    /// </summary>
    /// <param name="especialidadId">ID de la especialidad completada</param>
    public void SetupCongratulations(int especialidadId)
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
            string congratulationsText = $"¡HAS COMPLETADO LA ESPECIALIDAD DE {especialidadName.ToUpper()}!";
            titleText.text = congratulationsText;
        }
        else
        {
            Debug.LogWarning("No se pudo encontrar el elemento title-text en la UI");
        }
    }

    /// <summary>
    /// Actualiza los textos de las recompensas
    /// </summary>
    private void UpdateRewardTexts()
    {
        if (coinRewardText != null)
            coinRewardText.text = coinsReward.ToString();

        if (targetRewardText != null)
            targetRewardText.text = targetReward.ToString();
    }

    /// <summary>
    /// Configura las recompensas que se mostrarán
    /// </summary>
    /// <param name="coins">Cantidad de monedas</param>
    /// <param name="targets">Cantidad de objetivos/puntos</param>
    public void SetRewards(int coins, int targets)
    {
        coinsReward = coins;
        targetReward = targets;
        UpdateRewardTexts();
    }

    /// <summary>
    /// Maneja el clic del botón continuar
    /// </summary>
    private void OnContinueButtonClicked()
    {
        Debug.Log("Botón continuar presionado");

        // Aquí puedes agregar la lógica para continuar
        // Por ejemplo: cambiar de escena, cerrar la pantalla, etc.
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
    /// Evento que se dispara cuando se completa la pantalla de felicitaciones
    /// </summary>
    public System.Action OnCongratulationsComplete;

    private void OnDisable()
    {
        // Limpiar eventos para evitar memory leaks
        if (continueButton != null)
        {
            continueButton.clicked -= OnContinueButtonClicked;
        }
    }

}