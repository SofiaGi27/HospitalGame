using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static UsuarioService;

public class CharacterSelectionUI : MonoBehaviour
{
    // Referencia al documento de UI Toolkit
    private UIDocument uiDocument;

    // Mapeo entre el nombre del elemento visual (tarjeta) y el índice del personaje
    private Dictionary<string, int> characterMap = new Dictionary<string, int>
    {
        { "character-card-5", 0 },
        { "character-card-7", 1 },
        { "character-card-10", 2 },
        { "character-card-14", 3 },
        { "character-card-11", 4 },
        { "character-card-19", 5 },
    };

     // Clips de sonido para ambientación y efectos
    [Header("Audio Clips")]
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private AudioSource audioSource;

    // Comentario nuevo para forzar commit
    void OnEnable()
    {
        // Se obtiene el root del documento UI
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        // Se accede al contenedor de tarjetas
        var cardsContainer = root.Q<VisualElement>("cards-container");

        if (cardsContainer == null)
        {
            Debug.LogError("No se encontró el contenedor de tarjetas: 'cards-container'");
            return;
        }

        // Se ajustan estilos visuales
        cardsContainer.style.flexWrap = Wrap.NoWrap;
        cardsContainer.style.overflow = Overflow.Hidden;
        cardsContainer.style.marginBottom = 0;
        cardsContainer.style.paddingBottom = 0;


        // Para cada tarjeta, se añaden eventos
        foreach (var pair in characterMap)
        {
            var card = cardsContainer.Q<VisualElement>(pair.Key);
            if (card == null)
            {
                Debug.LogWarning($"No se encontró la tarjeta con name: {pair.Key}");
                continue;
            }

            int characterIndex = pair.Value;

            // Eventos de hover
            card.RegisterCallback<MouseEnterEvent>(evt =>
            {
                PlayHover();
                card.AddToClassList("hovered-card");
            });

            card.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                card.RemoveFromClassList("hovered-card");
            });

            // Evento de click
            card.RegisterCallback<ClickEvent>(evt =>
            {
                PlayClick();

                // Quitar selección anterior
                foreach (var otherPair in characterMap)
                {
                    var otherCard = cardsContainer.Q<VisualElement>(otherPair.Key);
                    otherCard?.RemoveFromClassList("selected-card");
                }

                // Marcar esta como seleccionada
                card.AddToClassList("selected-card");

                // Se guarda el índice del personaje usando PlayerPrefs
                PlayerPrefs.SetInt("SelectedCharacterIndex", characterIndex);
                UserSession.Instance.SetCharacterSelected(characterIndex); // Guarda en el singleton
                SceneManager.LoadScene("Game");
            });
        }

        var prevButton = root.Q<Button>("prev-button");
        if (prevButton != null)
        {
            prevButton.clicked += () => { PlayClick(); OnPrevButtonClick(); };
            prevButton.RegisterCallback<MouseEnterEvent>(_ => PlayHover());
        }

        var nextButton = root.Q<Button>("next-button");
        if (nextButton != null)
        {
            nextButton.clicked += () => { PlayClick(); OnNextButtonClick(); };
            nextButton.RegisterCallback<MouseEnterEvent>(_ => PlayHover());
        }
    }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = ambientClip;
        audioSource.Play();
    }

    private void OnPrevButtonClick()
    {
        Debug.Log("Botón anterior presionado");
    }

    private void OnNextButtonClick()
    {
        Debug.Log("Botón siguiente presionado");
    }

    void PlayClick()
    {
        if (clickClip != null) audioSource.PlayOneShot(clickClip);
    }

    void PlayHover()
    {
        if (hoverClip != null) audioSource.PlayOneShot(hoverClip);
    }
}
