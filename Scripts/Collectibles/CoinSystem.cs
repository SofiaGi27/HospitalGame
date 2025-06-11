using Autodesk.Fbx;
using UnityEngine;
using UnityEngine.UIElements;
using static UsuarioService;

public class CoinSystem : MonoBehaviour
{
    [Header("Configuración de Monedas")]
    public int coinsCollected;
    public int pointsPerCoin = 100;
    public int totalScore = 0;

    private int id = UserSession.Instance.IdUsuario;
    UsuarioService servicio ;

    [Header("UI Document")]
    public UIDocument uiDocument;
    
    // Referencias a elementos UI
    private Label coinCountLabel;
    private VisualElement coinIcon;
    
    void Start()
    {
        MySQLManager dbManager = FindAnyObjectByType<MySQLManager>();
        servicio = dbManager.usuarioService;
        totalScore = MonedasActuales(id);
        SetupUI();
    }
    
    void SetupUI()
    {
        if (uiDocument == null)
        {
            // Buscar UIDocument en el GameObject actual o en sus hijos
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                uiDocument = GetComponentInChildren<UIDocument>();
            }
        }
        
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            
            // Obtener referencias a los elementos UI
            coinCountLabel = root.Q<Label>("coin-count");
            coinIcon = root.Q<VisualElement>("coin-icon");
            
            // Actualizar la UI inicial
            UpdateUI();
        }
        else
        {
            Debug.LogError("UIDocument no encontrado. Asegúrate de asignar el UIDocument en el inspector.");
        }
    }
    
    public void CollectCoin()
    {
        coinsCollected++;
        totalScore += pointsPerCoin;
        //Agregar monedas a bbdd
        servicio.AddCoins(id,pointsPerCoin);
        UpdateUI();
        
        // Efecto visual al recolectar
        AnimateCoinCollection();
        
        Debug.Log($"Moneda recolectada! Total: {coinsCollected}, Puntuación: {totalScore}");
    }
    
    void UpdateUI()
    {
        if (coinCountLabel != null)
        {
            coinCountLabel.text = totalScore.ToString();

        }
    }
    int MonedasActuales(int id)
    {
        Usuario usuario = servicio.Seleccionar(id);
        coinsCollected = usuario.Monedas;
        return coinsCollected;
    }
    void AnimateCoinCollection()
    {
        if (coinIcon != null)
        {
            // Animación de pulso para el icono
            coinIcon.style.scale = new StyleScale(new Scale(Vector3.one * 1.3f));
            StartCoroutine(RestoreCoinIconScale());
        }
        
        if (coinCountLabel != null)
        {
            // Animación para el texto
            coinCountLabel.style.scale = new StyleScale(new Scale(Vector3.one * 1.2f));
            coinCountLabel.style.color = new StyleColor(new Color(1f, 0.84f, 0f)); // Dorado
            StartCoroutine(RestoreTextScale());
        }
    }
    
    System.Collections.IEnumerator RestoreCoinIconScale()
    {
        yield return new WaitForSeconds(0.15f);
        if (coinIcon != null)
        {
            coinIcon.style.scale = new StyleScale(new Scale(Vector3.one));
        }
    }
    
    System.Collections.IEnumerator RestoreTextScale()
    {
        yield return new WaitForSeconds(0.15f);
        if (coinCountLabel != null)
        {
            coinCountLabel.style.scale = new StyleScale(new Scale(Vector3.one));
            coinCountLabel.style.color = new StyleColor(Color.white);
        }
    }
    
    // Método para usar desde otros scripts
    public void AddCoins(int amount)
    {
        coinsCollected += amount;
        totalScore += amount * pointsPerCoin;
        UpdateUI();
        
        // Animar cuando se añaden monedas
        AnimateCoinCollection();
    }
    
    // Método para gastar monedas
    public bool SpendCoins(int amount)
    {
        if (coinsCollected >= amount)
        {
            coinsCollected -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }
    
    // Getters
    public int GetCoins() => coinsCollected;
    public int GetScore() => totalScore;
}