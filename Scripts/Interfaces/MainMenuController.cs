using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Texture2D backgroundTexture;

    [Header("Iconos de Volumen")]
    [SerializeField] private Texture2D volumenActivadoIcono;
    [SerializeField] private Texture2D volumenDesactivadoIcono;

    [Header("Sonidos")]
    [SerializeField] private AudioClip sonidoAmbiente;
    [SerializeField] private AudioClip sonidoClick;
    [SerializeField] private AudioClip sonidoHover;

    [Header("Configuración de Animaciones")]
    [SerializeField] private float duracionAnimacion = 0.4f;
    [SerializeField] private float escalaHover = 1.1f;

    private AudioSource audioSource;
    private Button playButton, volumeButton, settingsButton, rankingButton;
    private VisualElement settingsPanel;
    private Slider volumeSlider;
    private Label volumeValueLabel;
    private Button closeSettingsButton;

    private float volumenActual = 1f;
    private bool panelAbierto = false;

    private void OnEnable()
    {
        InicializarAudio();
        InicializarUI();
        ConfigurarEventos();
        ActualizarIconoVolumen();
    }

    private void InicializarAudio()
    {
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        if (sonidoAmbiente != null)
        {
            audioSource.clip = sonidoAmbiente;
            audioSource.loop = true;
            audioSource.volume = volumenActual;
            audioSource.Play();
        }
    }

    private void InicializarUI()
    {
        var root = uiDocument.rootVisualElement;
        root.style.backgroundImage = new StyleBackground(backgroundTexture);

        playButton = root.Q<Button>("PlayButton");
        volumeButton = root.Q<Button>("VolumeButton");
        settingsButton = root.Q<Button>("SettingsButton");
        rankingButton = root.Q<Button>("RankingButton");

        settingsPanel = root.Q<VisualElement>("SettingsPanel");
        volumeSlider = root.Q<Slider>("VolumeSlider");
        volumeValueLabel = root.Q<Label>("VolumeValueLabel");
        closeSettingsButton = root.Q<Button>("CloseSettingsButton");

        settingsPanel.style.display = DisplayStyle.None;
        settingsPanel.style.scale = new StyleScale(new Scale(Vector3.zero));
        settingsPanel.style.opacity = 0f;

        volumeSlider.value = volumenActual;
        volumeValueLabel.text = $"{Mathf.RoundToInt(volumenActual * 100)}%";
    }

    private void ConfigurarEventos()
    {
        playButton.clicked += () =>
        {
            ReproducirSonidoClick();
            AnimarBotonClick(playButton, StartGame);
        };

        volumeButton.clicked += () =>
        {
            ReproducirSonidoClick();
            AnimarBotonClick(volumeButton, CambiarVolumen);
        };

        settingsButton.clicked += () =>
        {
            ReproducirSonidoClick();
            AnimarBotonClick(settingsButton, () => ToggleSettingsPanel(true));
        };

        rankingButton.clicked += () =>
        {
            ReproducirSonidoClick();
            AnimarBotonClick(rankingButton, () => SceneManager.LoadScene("RankingScene"));
        };

        ConfigurarEventosHover(playButton);
        ConfigurarEventosHover(volumeButton);
        ConfigurarEventosHover(settingsButton);
        ConfigurarEventosHover(rankingButton);
        ConfigurarEventosHover(closeSettingsButton);

        volumeSlider.RegisterValueChangedCallback(evt =>
        {
            volumenActual = evt.newValue;
            audioSource.volume = volumenActual;
            volumeValueLabel.text = $"{Mathf.RoundToInt(volumenActual * 100)}%";
            ActualizarIconoVolumen();
            AnimarElemento(volumeValueLabel, 1.2f, 0.2f);
        });

        closeSettingsButton.clicked += () =>
        {
            ReproducirSonidoClick();
            ToggleSettingsPanel(false);
        };
    }

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

    private void AnimarElemento(VisualElement elemento, float escalaMaxima, float duracion)
    {
        StartCoroutine(AnimacionEscala(elemento, escalaMaxima, duracion));
    }

    private IEnumerator AnimacionEscala(VisualElement elemento, float escalaMaxima, float duracion)
    {
        elemento.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(duracion * 0.5f) });
        elemento.style.scale = new StyleScale(new Scale(Vector3.one * escalaMaxima));

        yield return new WaitForSeconds(duracion * 0.5f);

        elemento.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(duracion * 0.5f) });
        elemento.style.scale = new StyleScale(new Scale(Vector3.one));
    }

    private void ToggleSettingsPanel(bool mostrar)
    {
        if (panelAbierto == mostrar) return;

        panelAbierto = mostrar;
        StartCoroutine(AnimarPanel(mostrar));
    }

    private IEnumerator AnimarPanel(bool mostrar)
    {
        if (mostrar)
        {
            settingsPanel.style.display = DisplayStyle.Flex;
            settingsPanel.RemoveFromClassList("panel-hide");
            settingsPanel.AddToClassList("panel-show");

            settingsPanel.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(duracionAnimacion) });
            settingsPanel.style.scale = new StyleScale(new Scale(Vector3.one));
            settingsPanel.style.opacity = 1f;
            settingsPanel.style.translate = new StyleTranslate(new Translate(new Length(-50, LengthUnit.Percent), new Length(0)));
        }
        else
        {
            settingsPanel.RemoveFromClassList("panel-show");
            settingsPanel.AddToClassList("panel-hide");

            settingsPanel.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(duracionAnimacion * 0.7f) });
            settingsPanel.style.scale = new StyleScale(new Scale(Vector3.one * 0.8f));
            settingsPanel.style.opacity = 0f;

            yield return new WaitForSeconds(duracionAnimacion * 0.7f);

            settingsPanel.style.display = DisplayStyle.None;
        }
    }

    private void CambiarVolumen()
    {
        volumenActual = volumenActual < 0.2f ? 1f : 0f;
        audioSource.volume = volumenActual;

        volumeSlider.value = volumenActual;
        volumeValueLabel.text = $"{Mathf.RoundToInt(volumenActual * 100)}%";
        ActualizarIconoVolumen();

        AnimarElemento(volumeButton, 1.3f, 0.3f);
        AnimarElemento(volumeValueLabel, 1.2f, 0.2f);
    }

    private void StartGame()
    {
        Debug.Log("¡Iniciando juego!");
        SceneManager.LoadScene("LoginScene");
    }

    private IEnumerator TransicionEscena(string nombreEscena)
    {
        var root = uiDocument.rootVisualElement;
        root.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0.5f) });
        root.style.opacity = 0f;

        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene(nombreEscena);
    }

    private void ActualizarIconoVolumen()
    {
        if (volumeButton != null)
        {
            var icono = volumenActual < 0.2f ? volumenDesactivadoIcono : volumenActivadoIcono;
            volumeButton.style.backgroundImage = new StyleBackground(icono);
            AnimarElemento(volumeButton, 1.1f, 0.2f);
        }
    }

    private void ReproducirSonidoClick()
    {
        if (audioSource != null && sonidoClick != null && volumenActual > 0.1f)
            audioSource.PlayOneShot(sonidoClick, volumenActual * 0.7f);
    }

    private void ReproducirSonidoHover()
    {
        if (audioSource != null && sonidoHover != null && volumenActual > 0.1f)
            audioSource.PlayOneShot(sonidoHover, volumenActual * 0.5f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && panelAbierto)
        {
            ToggleSettingsPanel(false);
        }
    }

    private void OnDisable()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
