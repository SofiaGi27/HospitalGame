using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UsuarioService;

public class CongratulationsScreenController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("Rewards")]
    private int coinsReward ;
    private int targetReward;

    private int id = UserSession.Instance.IdUsuario;

    private VisualElement root;
    private Label titleText;
    private Label coinRewardText;
    private Label targetRewardText;
    private Button continueButton;
   
    // Sonido
    public AudioClip ambientMusicClip;
    public AudioClip buttonClickClip;

    private AudioSource ambientAudioSource;
    private AudioSource sfxAudioSource;

    private Dictionary<string, string> especialidadImagenes = new Dictionary<string, string>()
{
    { "Cardiología", "heart-happy" },
    { "Nefrología", "kidney-happy" },
    { "Digestivo", "stomach-happy" },
};

    private void Awake()
    {
        if (uiDocument == null)
        uiDocument = GetComponent<UIDocument>();

    // Obtener los dos AudioSource del GameObject
    AudioSource[] sources = GetComponents<AudioSource>();
    if (sources.Length >= 2)
    {
        ambientAudioSource = sources[0]; // El primero es para música ambiental
        sfxAudioSource = sources[1];     // El segundo es para efectos (clic botón)
    }

    // Configurar música ambiental
    if (ambientAudioSource != null && ambientMusicClip != null)
    {
        ambientAudioSource.clip = ambientMusicClip;
        ambientAudioSource.loop = true;
        ambientAudioSource.playOnAwake = false;
        ambientAudioSource.volume = 0.5f; 
        ambientAudioSource.Play();
    }
       
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

        // Limpiar los PlayerPrefs despu�s de usarlos
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
            Debug.LogError("UIDocument no est� asignado en CongratulationsScreenController");
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

        // Obtener el bot�n de continuar
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
    /// Configura la pantalla de felicitaciones para una especialidad espec�fica
    /// </summary>
    /// <param name="especialidadId">ID de la especialidad completada</param>
    public void SetupCongratulations(int especialidadId)
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
            Usuario usuario = MySQLManager.Instance.usuarioService.Seleccionar(id);
            coinsReward = usuario.Monedas;

            if (string.IsNullOrEmpty(especialidadName))
            {
                Debug.LogWarning($"No se pudo obtener el nombre de la especialidad con ID: {especialidadId}");
                especialidadName = "ESPECIALIDAD DESCONOCIDA";
            }

            // Actualizar el texto del t�tulo
            UpdateTitleText(especialidadName);
            UpdateEspecialidadImage(especialidadName);

            UpdateRewardTexts();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al obtener el nombre de la especialidad: {ex.Message}");
            UpdateTitleText("ESPECIALIDAD");
        }
    }
    private void UpdateEspecialidadImage(string especialidadName)
    {
        var imageElement = root.Q<UnityEngine.UIElements.Image>("specialty-image");
        if (imageElement == null)
        {
            Debug.LogError("❌ No se encontró el elemento con name='specialty-image'");
            return;
        }

        if (!especialidadImagenes.TryGetValue(especialidadName, out string imageKey))
        {
            Debug.LogWarning($"⚠️ No se encontró imagen para la especialidad '{especialidadName}'");
            return;
        }

        Texture2D tex = Resources.Load<Texture2D>($"Sprites/{imageKey}");
        if (tex == null)
        {
            Debug.LogError($"❌ No se pudo cargar Resources/Sprites/{imageKey}");
            return;
        }

        imageElement.image = tex;
        imageElement.scaleMode = ScaleMode.ScaleToFit;

        Debug.Log($"✅ Imagen '{imageKey}' cargada y asignada correctamente.");
    }



    /// <summary>
    /// Actualiza el texto del t�tulo con el nombre de la especialidad
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
    /// Configura las recompensas que se mostrar�n
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
    /// Maneja el clic del bot�n continuar
    /// </summary>
    private void OnContinueButtonClicked()
    {
        if (buttonClickClip != null && sfxAudioSource != null)
    {
        sfxAudioSource.PlayOneShot(buttonClickClip);
    }

    Debug.Log("Botón continuar presionado");
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