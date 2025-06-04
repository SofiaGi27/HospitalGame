using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CharacterSelectionUI : MonoBehaviour
{
    private UIDocument uiDocument;

    // Mapeamos los nombres de tarjetas a índices de personajes
    private Dictionary<string, int> characterMap = new Dictionary<string, int>
    {
        { "character-card-5", 0 },
        { "character-card-7", 1 },
        { "character-card-10", 2 },
        { "character-card-14", 3 },
        { "character-card-11", 4 },
        { "character-card-19", 5 },
    };

    // Sonido
    [Header("Audio Clips")]
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private AudioSource audioSource;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        var cardsContainer = root.Q<VisualElement>("cards-container");

        if (cardsContainer == null)
        {
            Debug.LogError("No se encontró el contenedor de tarjetas: 'cards-container'");
            return;
        }

        cardsContainer.style.flexWrap = Wrap.NoWrap;
        cardsContainer.style.overflow = Overflow.Hidden;
        cardsContainer.style.marginBottom = 0;
        cardsContainer.style.paddingBottom = 0;

        foreach (var pair in characterMap)
        {
            var card = cardsContainer.Q<VisualElement>(pair.Key);
            if (card == null)
            {
                Debug.LogWarning($"No se encontró la tarjeta con name: {pair.Key}");
                continue;
            }

            int characterIndex = pair.Value;
            card.RegisterCallback<ClickEvent>(evt =>
            {
                PlayClick();
                PlayerPrefs.SetInt("SelectedCharacterIndex", characterIndex);
                SceneManager.LoadScene("Game");
            });

            card.RegisterCallback<MouseEnterEvent>(_ => PlayHover());
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

    // Métodos de sonido
    void PlayClick()
    {
        if (clickClip != null) audioSource.PlayOneShot(clickClip);
    }

    void PlayHover()
    {
        if (hoverClip != null) audioSource.PlayOneShot(hoverClip);
    }
}
