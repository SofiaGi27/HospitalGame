using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class RankingController : MonoBehaviour
{
    [Header("Referencias")]
    public UIDocument uiDocument;

    // Sonido
    [SerializeField] public AudioClip ambientClip;
    [SerializeField] public AudioClip clickClip;
    [SerializeField] private AudioClip sonidoHover;

    [Header("Configuraci�n de Animaciones")]
    [SerializeField] private float duracionAnimacion = 0.4f;
    [SerializeField] private float escalaHover = 1.1f;

    private float volumenActual = 1f;

    private AudioSource ambientSource;
    private AudioSource sfxSource;

    private PuntajeService puntajeService;
    private VisualElement rankingList;
    private List<VisualElement> rankingItems;
    private Button backButton;

    private MySQLManager mysqlManager; // Referencia al MySQLManager
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

    void Start()
    {
        if (mysqlManager == null)
        {
            mysqlManager = FindAnyObjectByType<MySQLManager>();
            if (mysqlManager == null)
            {
                Debug.LogError("No MySQLManager found in scene! Please assign one in the inspector or ensure one exists in the scene.");
                return;
            }
        }

        // Obtener los servicios del MySQLManager
        puntajeService = mysqlManager.puntajeService;
        // Obtener referencias a los elementos UI
        ObtenerReferenciasUI();

        // Cargar y mostrar el ranking
        CargarRanking();
    }

    private void ObtenerReferenciasUI()
    {
        var root = uiDocument.rootVisualElement;
        rankingList = root.Q<VisualElement>(className: "ranking-list");

        // Obtener todos los elementos de ranking
        rankingItems = rankingList.Query<VisualElement>(className: "ranking-item").ToList();

        backButton = root.Q<Button>("back-game");
        if (backButton == null) Debug.LogWarning("back-button not found!");
        SetupButtonEvents();

        Debug.Log($"Encontrados {rankingItems.Count} elementos de ranking");
    }

    private void SetupButtonEvents()
    {
        //Boton para regresar a game
        backButton.clicked += () =>
        {
            ReproducirSonidoClick();
            AnimarBotonClick(backButton, onBackGameClicked);
        };
        ConfigurarEventosHover(backButton);
    }
    private void onBackGameClicked()
    {
        SceneManager.LoadScene("MenuGame");

    }
    private void AnimarBotonClick(Button button, System.Action callback = null)
    {
        StartCoroutine(AnimacionClick(button, callback));
    }
    private IEnumerator AnimacionClick(Button button, System.Action callback)
    {
        button.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0.1f) });
        button.style.scale = new StyleScale(new Scale(Vector3.one * 0.95f));

        yield return new WaitForSeconds(0.1f);

        button.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0.2f) });
        button.style.scale = new StyleScale(new Scale(Vector3.one));

        yield return new WaitForSeconds(0.1f);

        callback?.Invoke();
    }
    private void CargarRanking()
    {
        try
        {
            // Obtener los 5 mejores puntajes - Usar la clase del PuntajeService
            List<PuntajeService.Puntaje> topPuntajes = puntajeService.ObtenerTop5Puntajes();

            // Actualizar cada elemento del ranking
            for (int i = 0; i < rankingItems.Count; i++)
            {
                if (i < topPuntajes.Count)
                {
                    // Hay datos para esta posici�n
                    ActualizarElementoRanking(rankingItems[i], topPuntajes[i], i + 1);
                }
                else
                {
                    // No hay datos para esta posici�n, mostrar como vac�o
                    MostrarElementoVacio(rankingItems[i], i + 1);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar el ranking: {e.Message}");
            MostrarRankingVacio();
        }
    }

    private void ActualizarElementoRanking(VisualElement elemento, PuntajeService.Puntaje puntaje, int posicion)
    {
        // Buscar el label del nombre
        var nombreLabel = elemento.Q<Label>(className: "ranking-name");
        if (nombreLabel != null)
        {
            nombreLabel.text = puntaje.NombreUsuario; // Usar NombreUsuario
        }

        // Buscar el label del puntaje
        var scoreLabels = elemento.Query<Label>(className: "ranking-score-text").ToList();

        foreach (var scoreLabel in scoreLabels)
        {
            // Verificar que no sea una medalla 
            var parent = scoreLabel.parent;
            if (parent != null && !parent.ClassListContains("ranking-position"))
            {
                scoreLabel.text = puntaje.scoreTotal.ToString();
                break;
            }
        }

        // Actualizar el n�mero de posici�n en la medalla
        ActualizarMedalla(elemento, posicion);
    }

    private void ActualizarMedalla(VisualElement elemento, int posicion)
    {
        var positionContainer = elemento.Q<VisualElement>(className: "ranking-position");
        if (positionContainer != null)
        {
            var medallaLabel = positionContainer.Q<Label>(className: "ranking-score-text");
            if (medallaLabel != null)
            {
                medallaLabel.text = posicion.ToString();
            }
        }
    }

    private bool EsMedalla(Label label, int posicion)
    {
        // Verificar si el label corresponde a una medalla bas�ndose en su imagen de fondo
        var backgroundImage = label.style.backgroundImage.value;
        if (backgroundImage.sprite != null)
        {
            string spriteName = backgroundImage.sprite.name.ToLower();
            return spriteName.Contains("gold") || spriteName.Contains("silver") || spriteName.Contains("bronze");
        }
        return false;
    }

    private void MostrarElementoVacio(VisualElement elemento, int posicion)
    {
        // Mostrar nombre vac�o o placeholder
        var nombreLabel = elemento.Q<Label>(className: "ranking-name");
        if (nombreLabel != null)
        {
            nombreLabel.text = "---";
        }

        // Mostrar puntaje como 0 (label que NO est� en ranking-position)
        var scoreLabels = elemento.Query<Label>(className: "ranking-score-text").ToList();
        foreach (var scoreLabel in scoreLabels)
        {
            var parent = scoreLabel.parent;
            if (parent != null && !parent.ClassListContains("ranking-position"))
            {
                scoreLabel.text = "0";
                break;
            }
        }

        // Mantener el n�mero de posici�n
        ActualizarMedalla(elemento, posicion);
    }

    private void MostrarRankingVacio()
    {
        for (int i = 0; i < rankingItems.Count; i++)
        {
            MostrarElementoVacio(rankingItems[i], i + 1);
        }
    }

    // M�todo p�blico para refrescar el ranking (�til si se actualiza desde otra escena)
    public void RefrescarRanking()
    {
        CargarRanking();
    }

    //Configuraci�n de sonidos
    private void ConfigurarEventosHover(Button button)
    {
        if (button == null) return;

        button.RegisterCallback<MouseEnterEvent>(_ => OnHoverEnter(button));
        button.RegisterCallback<MouseLeaveEvent>(_ => OnHoverExit(button));
    }

    private void OnHoverEnter(Button button)
    {
        ReproducirSonidoHover();
        button.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(duracionAnimacion * 0.5f) });
        button.style.scale = new StyleScale(new Scale(Vector3.one * escalaHover));
        button.AddToClassList("button-hover");
    }

    private void OnHoverExit(Button button)
    {
        button.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(duracionAnimacion * 0.3f) });
        button.style.scale = new StyleScale(new Scale(Vector3.one));
        button.RemoveFromClassList("button-hover");
    }
    private void ReproducirSonidoHover()
    {
        if (ambientSource != null && sonidoHover != null && volumenActual > 0.1f)
            ambientSource.PlayOneShot(sonidoHover, volumenActual * 0.5f);
    }
    private void ReproducirSonidoClick()
    {
        if (ambientSource != null && clickClip != null && volumenActual > 0.1f)
            ambientSource.PlayOneShot(clickClip, volumenActual * 0.7f);
    }

}

