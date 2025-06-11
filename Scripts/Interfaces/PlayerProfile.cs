using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class PlayerProfileController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private UIDocument uiDocument;

    private MySQLManager mysqlManager; // Referencia al MySQLManager


    // Services 
    private UsuarioService usuarioService;
    private PuntajeService puntajeService;
    private EspecialidadService especialidadService;

    // UI Elements
    private VisualElement playerProfileContainer;
    private VisualElement avatarImage;
    private Label playerName;
    private Label playerId;
    private Label moneyValue;
    private Label globalScoreValue;
    private Button backButton;

    // Specialty elements 
    private VisualElement specialtyContainer;
    private Dictionary<int, VisualElement> specialtyRows = new Dictionary<int, VisualElement>();
    private Dictionary<int, Label> specialtyScoreLabels = new Dictionary<int, Label>();

    // Buttons
    private VisualElement avatarPlusButton;
    private VisualElement namePlusButton;

    // ID del usuario y rol actual
    int rolUsuario = UserSession.Instance.RolUsuario;
    int idUsuario = UserSession.Instance.IdUsuario;
    int character = UserSession.Instance.CharacterSelected;


    // Data
    private List<EspecialidadService.Especialidad> especialidades;

    // Sonido
    public AudioClip ambientClip;
    public AudioClip clickClip;

    private AudioSource ambientSource;
    private AudioSource sfxSource;

    private Dictionary<string, string> especialidadImagenes = new Dictionary<string, string>()
{
    { "Cardiología", "heart-icon" },
    { "Nefrología", "kidney-icon" },
    { "Digestivo", "stomach-icon" },
};

    void Awake()
    {
        // Crear dos AudioSources para separar ambiente y efectos
        ambientSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        // Configurar audio de ambiente
        ambientSource.clip = ambientClip;
        ambientSource.loop = true;
        ambientSource.volume = 0.5f;
        ambientSource.Play();
    }

    public void PlayClick()
    {
        sfxSource.PlayOneShot(clickClip, 1.0f);
    }





    private void Start()
    {
        Debug.Log("=== PlayerProfileController Start ===");
        InitializeServices();
        InitializeUI();
        LoadPlayerData();
    }

    private void InitializeServices()
    {
        Debug.Log("Initializing services...");

        // Si no hay MySQLManager asignado, buscar uno en la escena
        if (mysqlManager == null)
        {
            mysqlManager = FindObjectOfType<MySQLManager>();
            if (mysqlManager == null)
            {
                Debug.LogError("No MySQLManager found in scene! Please assign one in the inspector or ensure one exists in the scene.");
                return;
            }
        }

        // Obtener los servicios del MySQLManager
        usuarioService = mysqlManager.usuarioService;
        puntajeService = mysqlManager.puntajeService;
        especialidadService = mysqlManager.especialidadService;

        // Verificar que los servicios est�n inicializados
        if (usuarioService == null) Debug.LogError("UsuarioService is null in MySQLManager!");
        if (puntajeService == null) Debug.LogError("PuntajeService is null in MySQLManager!");
        if (especialidadService == null) Debug.LogError("EspecialidadService is null in MySQLManager!");

        Debug.Log($"Services initialized - Usuario: {usuarioService != null}, Puntaje: {puntajeService != null}, Especialidad: {especialidadService != null}");
    }

    private void InitializeUI()
    {
        Debug.Log("Initializing UI...");

        if (uiDocument == null)
        {
            Debug.LogError("UIDocument is null! Make sure to assign it in the inspector.");
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("No UIDocument component found on this GameObject!");
                return;
            }
        }

        var root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("Root visual element is null!");
            return;
        }

        Debug.Log($"Root element found. Children count: {root.childCount}");

        // Obtener el contenedor principal
        playerProfileContainer = root.Q<VisualElement>("player-profile-container");
        if (playerProfileContainer == null)
            Debug.LogError("player-profile-container not found!");
        else
            Debug.Log("player-profile-container found successfully");

        // Obtener referencia a los elementos visuales
        avatarImage = root.Q<VisualElement>("avatar-image");
        if (avatarImage == null) Debug.LogWarning("avatar-image not found!");

        playerName = root.Q<Label>("player-name");
        if (playerName == null) Debug.LogWarning("player-name not found!");

        playerId = root.Q<Label>("player-id");
        if (playerId == null) Debug.LogWarning("player-id not found!");

        var moneyStat = root.Q<VisualElement>("money-stat");
        if (moneyStat == null)
        {
            Debug.LogWarning("money-stat not found!");
            moneyValue = null;
        }
        else
        {
            moneyValue = moneyStat.Q<Label>("stat-value");
            if (moneyValue == null) Debug.LogWarning("stat-value inside money-stat not found!");
        }

        var scoreStat = root.Q<VisualElement>("score-stat");
        if (scoreStat == null)
        {
            Debug.LogWarning("score-stat not found!");
            globalScoreValue = null;
        }
        else
        {
            globalScoreValue = scoreStat.Q<Label>("score-value");
            if (globalScoreValue == null) Debug.LogWarning("score-value inside score-stat not found!");
        }

        // Contenedor de especialidad
        specialtyContainer = root.Q<VisualElement>("specialty-container");
        if (specialtyContainer == null)
            Debug.LogError("specialty-container not found!");
        else
            Debug.Log("specialty-container found successfully");

        // Botones
        avatarPlusButton = root.Q<VisualElement>("plus-icon");
        if (avatarPlusButton == null) Debug.LogWarning("plus-icon not found!");

        namePlusButton = root.Q<VisualElement>("plus-button");
        if (namePlusButton == null) Debug.LogWarning("plus-button not found!");

        backButton = root.Q<Button>("back-game");
        if (backButton == null) Debug.LogWarning("back-button not found!");

        // Eventos de botones
        SetupButtonEvents();

        Debug.Log("UI initialization completed");
    }

    private void SetupButtonEvents()
    {
        // Boton para cambiar el avatar
        if (avatarPlusButton != null)
        {
            avatarPlusButton.RegisterCallback<ClickEvent>(evt => OnAvatarChangeClicked());
            Debug.Log("Avatar button event registered");
        }

        // Boton para editar el nombre
        if (namePlusButton != null)
        {
            namePlusButton.RegisterCallback<ClickEvent>(evt => OnNameEditClicked());
            Debug.Log("Name button event registered");
        }

        //Boton para regresar a game
        if (backButton != null)
        {
            backButton.RegisterCallback<ClickEvent>(evt => {
                onBackGameClicked();
                PlayClick();
            });
            Debug.Log("Back button event registered");
        }

    }

    private async void LoadPlayerData()
    {
        Debug.Log("=== Loading Player Data ===");

        // Verificar que los servicios est�n disponibles
        if (usuarioService == null || puntajeService == null || especialidadService == null)
        {
            Debug.LogError("Cannot load player data - services are not initialized properly");
            return;
        }

        try
        {
            // Obtiene el id del usuario por UserSession

            Debug.Log($"Using user ID: {idUsuario}");

            if (idUsuario <= 0)
            {
                Debug.LogError("No valid user ID found in session");
                return;
            }

            // Carga de especialidades
            Debug.Log("Loading specialties...");
            especialidades = await especialidadService.GetEspecialidadesAsync(rolUsuario);

            if (especialidades != null)
            {
                Debug.Log($"Loaded {especialidades.Count} specialties");
                CreateSpecialtyUI();
            }
            else
            {
                Debug.LogError("Failed to load specialties - returned null");
            }

            // Carga la informaci�n del usuario
            Debug.Log("Loading user data...");
            var userData = await usuarioService.GetUserByIdAsync(idUsuario);
            if (userData != null)
            {
                Debug.Log($"User data loaded: {userData.Name}");
                UpdatePlayerInfo(userData);
            }
            else
            {
                Debug.LogError("Failed to load user data - returned null");
            }

            // Carga los puntajes
            Debug.Log("Loading scores...");
            var scores = await puntajeService.GetPlayerScoresByIdAsync(idUsuario);
            if (scores != null)
            {
                Debug.Log($"Global score loaded: {scores.GlobalScore}");
                UpdateScores(scores);
            }

            var specialtyScores = await puntajeService.GetSpecialtyScoresByIdAsync(idUsuario);
            if (specialtyScores != null)
            {
                Debug.Log($"Specialty scores loaded: {specialtyScores.ScoresBySpecialty.Count} entries");
                UpdateSpecialtyScores(specialtyScores);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading player data: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void CreateSpecialtyUI()
    {
        Debug.Log("=== Creating Specialty UI ===");

        if (specialtyContainer == null)
        {
            Debug.LogError("Cannot create specialty UI - specialty container is null");
            return;
        }

        if (especialidades == null || especialidades.Count == 0)
        {
            Debug.LogWarning("No specialties to create UI for");
            return;
        }

        // Limpia las filas de especialidades
        specialtyContainer.Clear();
        specialtyRows.Clear();
        specialtyScoreLabels.Clear();

        Debug.Log($"Creating UI for {especialidades.Count} specialties");

        // Crea la interfaz para cada especialidad
        foreach (var especialidad in especialidades)
        {
            Debug.Log($"Creating row for specialty: {especialidad.Name} (ID: {especialidad.Id})");
            CreateSpecialtyRow(especialidad);
        }

        Debug.Log($"Specialty container now has {specialtyContainer.childCount} children");
    }

    private void CreateSpecialtyRow(EspecialidadService.Especialidad especialidad)
    {
        // Contenedor principal
        var row = new VisualElement();
        row.name = $"specialty-row-{especialidad.Id}";
        row.AddToClassList("specialty-row");

        // Informaci�n de las especialidades
        var specialtyInfo = new VisualElement();
        specialtyInfo.name = "specialty-info";
        specialtyInfo.AddToClassList("specialty-info");

        // Contenedor del icono
        var iconContainer = new VisualElement();
        iconContainer.name = $"specialty-icon-{especialidad.Id}";
        iconContainer.AddToClassList("specialty-icon");

        // Obtener el icono para esa especialidad
        if (especialidadImagenes.TryGetValue(especialidad.Name, out string imageKey))
        {
            Texture2D texture = Resources.Load<Texture2D>($"Sprites/{imageKey}");

            if (texture != null)
            {
                iconContainer.style.backgroundImage = new StyleBackground(texture);
            }
            else
            {
                Debug.LogError($"No se pudo cargar la imagen: Resources/Sprites/{imageKey}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ No se encontró imagen para la especialidad '{especialidad.Name}'");
            return;
        }


        // Label por especialidad
        var nameLabel = new Label(especialidad.Name);
        nameLabel.name = "specialty-name";
        nameLabel.AddToClassList("specialty-name");

        // Agregar icono y nombre
        specialtyInfo.Add(iconContainer);
        specialtyInfo.Add(nameLabel);

        // Label del puntaje
        var scoreLabel = new Label("0");
        scoreLabel.name = "specialty-score";
        scoreLabel.AddToClassList("specialty-score");
        scoreLabel.AddToClassList("no-answered");

        // A�adir elementos a la fila
        row.Add(specialtyInfo);
        row.Add(scoreLabel);

        // A�adir fila al contenedor
        specialtyContainer.Add(row);

        // Guardar referencias
        specialtyRows[especialidad.Id] = row;
        specialtyScoreLabels[especialidad.Id] = scoreLabel;

        Debug.Log($"Created row for specialty {especialidad.Name}");
    }


    private void UpdatePlayerInfo(UserData userData)
    {
        Debug.Log("=== Updating Player Info ===");

        if (playerName != null)
        {
            playerName.text = userData.Name;
            Debug.Log($"Updated player name: {userData.Name}");
        }
        else
        {
            Debug.LogError("Cannot update player name - element is null");
        }

        if (playerId != null)
        {
            playerId.text = $"Player ID: {userData.PlayerId}";
            Debug.Log($"Updated player ID: {userData.PlayerId}");
        }
        else
        {
            Debug.LogError("Cannot update player ID - element is null");
        }

        if (moneyValue != null)
        {
            moneyValue.text = userData.Money.ToString();
            Debug.Log($"Updated money: {userData.Money}");
        }
        else
        {
            Debug.LogError("Cannot update money - element is null");
        }

        string avatarpath=SelectAvatarPath();
        UpdateAvatarImage(avatarpath);
    }

    private void UpdateScores(PlayerScores scores)
    {
        Debug.Log("=== Updating Scores ===");

        if (globalScoreValue != null)
        {
            globalScoreValue.text = scores.GlobalScore.ToString();
            Debug.Log($"Updated global score: {scores.GlobalScore}");
        }
        else
        {
            Debug.LogError("Cannot update global score - element is null");
        }
    }

    private void UpdateSpecialtyScores(SpecialtyScores specialtyScores)
    {
        Debug.Log("=== Updating Specialty Scores ===");

        if (specialtyScores == null || specialtyScoreLabels == null)
        {
            Debug.LogError("Cannot update specialty scores - data or labels are null");
            return;
        }

        // Actualiza el puntaje por especialidad
        foreach (var kvp in specialtyScores.ScoresBySpecialty)
        {
            int especialidadId = kvp.Key;
            int score = kvp.Value;

            Debug.Log($"Updating specialty {especialidadId} with score {score}");

            if (specialtyScoreLabels.ContainsKey(especialidadId))
            {
                var scoreLabel = specialtyScoreLabels[especialidadId];
                scoreLabel.text = score.ToString();
                scoreLabel.RemoveFromClassList("no-answered");
                scoreLabel.style.color = Color.white;
                Debug.Log($"Successfully updated specialty {especialidadId} score");
            }
            else
            {
                Debug.LogWarning($"No label found for specialty ID {especialidadId}");
            }
        }
    }
    private Dictionary<string, int> characterMap = new Dictionary<string, int>
    {
        { "character-5", 0 },
        { "character-10", 1 },
        { "character-7", 2 },
        { "character-11", 3 },
        { "character-19", 4 },
        { "character-14", 5 },
    };
    private void UpdateAvatarImage(string avatarPath)
    {
        Texture2D texture = Resources.Load<Texture2D>("Images/Characters/" + avatarPath);
        if (avatarImage != null)
        {
            if (texture != null)
            {
                avatarImage.style.backgroundImage = new StyleBackground(texture);
                Debug.Log($"Avatar updated: {avatarPath}");
            }
            else
            {
                Debug.LogWarning($"Could not load avatar texture at path: {avatarPath}");
            }
        }
    }
    private string SelectAvatarPath()
    {
        foreach (var kvp in characterMap)
        {
            if (kvp.Value == character)
            {
                return kvp.Key; // Retorna la clave correspondiente al valor
            }
        }
        Debug.LogWarning($"No se encontró una entrada en characterMap para el valor: {character}");
        return null;
    }


    #region Button Events

    private void OnAvatarChangeClicked()
    {
        Debug.Log("Avatar change clicked");
        //Por implementar
    }

    private void OnNameEditClicked()
    {
        Debug.Log("Name edit clicked");
        //Por implementar
    }

    private void onBackGameClicked()
    {
        SceneManager.LoadScene("Game");

    }
    #endregion

    #region Public Methods

    public void RefreshData()
    {
        LoadPlayerData();
    }

    public void UpdateMoney(int newAmount)
    {
        if (moneyValue != null)
            moneyValue.text = newAmount.ToString();
    }

    public void UpdateGlobalScore(int newScore)
    {
        if (globalScoreValue != null)
            globalScoreValue.text = newScore.ToString();
    }

    public void UpdateSpecialtyScore(int especialidadId, int newScore)
    {
        if (specialtyScoreLabels.ContainsKey(especialidadId))
        {
            var scoreLabel = specialtyScoreLabels[especialidadId];
            scoreLabel.text = newScore.ToString();
            scoreLabel.RemoveFromClassList("no-answered");
            scoreLabel.style.color = Color.white;
        }
    }

    #endregion
}

[System.Serializable]
public class SpecialtyIconData
{
    public int specialtyId;
    public string specialtyName;
    public Texture2D icon;
    public Color backgroundColor = Color.white;
}

#region Data Classes

[System.Serializable]
public class UserData
{
    public string Name;
    public int PlayerId;
    public int Money;
    public string AvatarPath;

    public override string ToString()
    {
        return $"UserData: Name={Name}, PlayerId={PlayerId}, Money={Money}, AvatarPath={AvatarPath}";
    }
}

[System.Serializable]
public class PlayerScores
{
    public int GlobalScore;
}

[System.Serializable]
public class SpecialtyScores
{
    public Dictionary<int, int> ScoresBySpecialty = new Dictionary<int, int>();

    // Propiedades de compatibilidad 
    public int CardiologyScore
    {
        get => ScoresBySpecialty.ContainsKey(1) ? ScoresBySpecialty[1] : 0;
        set => ScoresBySpecialty[1] = value;
    }

    public int DigestiveScore
    {
        get => ScoresBySpecialty.ContainsKey(2) ? ScoresBySpecialty[2] : 0;
        set => ScoresBySpecialty[2] = value;
    }

    public int NephrologyScore
    {
        get => ScoresBySpecialty.ContainsKey(3) ? ScoresBySpecialty[3] : 0;
        set => ScoresBySpecialty[3] = value;
    }


    
}



#endregion