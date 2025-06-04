using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GameOverSounds : MonoBehaviour
{
    public AudioClip hoverSound;      // Sonido al pasar el mouse
    public AudioClip selectSound;     // Sonido al hacer clic
    public AudioClip ambientSound;    // Sonido ambiente de fondo

    private AudioSource ambientSource;
    private AudioSource effectsSource;

    private bool hasHovered = false;

    void OnEnable()
    {
        // Crea dos AudioSources: uno para ambiente, otro para efectos
        SetupAudioSources();

        // Reproducir ambiente
        if (ambientSound != null)
        {
            ambientSource.clip = ambientSound;
            ambientSource.loop = true;
            ambientSource.volume = 0.5f; // Ajusta según necesidad
            ambientSource.Play();
        }

        // Buscar botón
        var root = GetComponent<UIDocument>().rootVisualElement;
        var retryButton = root.Q<Button>("Reintentar");

        if (retryButton != null)
        {
            retryButton.RegisterCallback<PointerEnterEvent>(ev => {
                if (!hasHovered)
                {
                    PlayHoverSound();
                    hasHovered = true;
                }
            });

            retryButton.RegisterCallback<PointerLeaveEvent>(ev => {
                hasHovered = false;
            });

            retryButton.RegisterCallback<ClickEvent>(ev => PlaySelectSound());
        }
        else
        {
            Debug.LogWarning("Botón 'Reintentar' no encontrado.");
        }
    }

    private void SetupAudioSources()
    {
        // Revisa si ya existen
        var sources = GetComponents<AudioSource>();
        if (sources.Length >= 2)
        {
            ambientSource = sources[0];
            effectsSource = sources[1];
        }
        else
        {
            // Crear dos si no existen
            ambientSource = gameObject.AddComponent<AudioSource>();
            effectsSource = gameObject.AddComponent<AudioSource>();
        }

        ambientSource.loop = true;
        ambientSource.playOnAwake = false;

        effectsSource.playOnAwake = false;
        effectsSource.loop = false;
    }

    private void PlayHoverSound()
    {
        if (hoverSound != null)
        {
            effectsSource.pitch = Random.Range(0.95f, 1.05f);
            effectsSource.PlayOneShot(hoverSound);
        }
    }

    private void PlaySelectSound()
    {
        if (selectSound != null)
        {
            effectsSource.pitch = 1f;
            effectsSource.PlayOneShot(selectSound);
        }
    }
}