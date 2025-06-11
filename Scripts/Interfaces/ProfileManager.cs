using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class ProfileManager : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;

    [Header("Profile Settings")]
    public string profileSceneName = "PlayerProfile"; // Aquí el nombre correcto de la escena

    private Button profileButton;
    int character = UserSession.Instance.CharacterSelected;
    private VisualElement avatarImage;

    void Start()
    {
        InitializeUI();
    }

    void InitializeUI()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument not found! Please assign it in the inspector.");
                return;
            }
        }

        var root = uiDocument.rootVisualElement;
        profileButton = root.Q<Button>("profile-button");

        if (profileButton == null)
        {
            Debug.LogError("Profile button not found in UXML!");
            return;
        }
        avatarImage = root.Q<VisualElement>("profile-image");

        string avatarpath = SelectAvatarPath();
        UpdateAvatarImage(avatarpath);
        profileButton.clicked += OnProfileClicked;
    }

    void OnProfileClicked()
    {
        Debug.Log("Cargando la escena PlayerProfile...");
        LoadProfileScene();
    }

    void LoadProfileScene()
    {
        if (Application.CanStreamedLevelBeLoaded(profileSceneName))
        {
            SceneManager.LoadScene(profileSceneName);
        }
        else
        {
            Debug.LogWarning($"La escena '{profileSceneName}' no está incluida en Build Settings.");
        }
    }

    void OnDestroy()
    {
        if (profileButton != null)
        {
            profileButton.clicked -= OnProfileClicked;
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
}
