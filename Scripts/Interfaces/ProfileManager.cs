using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class ProfileManager : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument uiDocument;

    [Header("Profile Settings")]
    public string profileSceneName = "PlayerProfile"; // Aquí el nombre correcto de la escena

    private Button profileButton;

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
}
