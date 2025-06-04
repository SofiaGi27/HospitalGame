using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class CharacterUIAudio : MonoBehaviour
{
    public AudioClip hoverSound;
    public AudioClip selectSound;
    public AudioClip ambientSound;  // El sonido ambiental

    private AudioSource audioSource;
    private AudioSource ambientAudioSource;  // Fuente para el sonido ambiental

    void OnEnable()
    {
        Debug.Log("OnEnable llamado en " + gameObject.name);

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogWarning("No se encontró AudioSource en " + gameObject.name);
        }

        // Obtener el UIDocument
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("No se encontró UIDocument en " + gameObject.name);
            return;
        }

        var root = uiDocument.rootVisualElement;

        // Buscar todos los elementos con la clase ".character-card"
        var cards = root.Query<VisualElement>(className: "character-card");

        cards.ForEach(card =>
        {
            // Hover = puntero entra
            card.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (hoverSound != null)
                    audioSource.PlayOneShot(hoverSound);
                else
                    Debug.LogWarning("hoverSound no está asignado");
            });

            // Click = selección
            card.RegisterCallback<ClickEvent>(_ =>
            {
                if (selectSound != null)
                    audioSource.PlayOneShot(selectSound);
                else
                    Debug.LogWarning("selectSound no está asignado");

                // Guardar personaje seleccionado
                var characterName = card.name; // ej: character-card-10
                PlayerPrefs.SetString("CharacterSelected", characterName);

                // Cargar escena
                SceneManager.LoadScene("NombreDeTuEscena");
            });
        });

        // Configurar y reproducir el sonido ambiental
        PlayAmbientSound();
    }

    private void PlayAmbientSound()
    {
        Debug.Log("PlayAmbientSound ejecutado");

        if (ambientSound == null)
        {
            Debug.LogWarning("ambientSound está vacío");
            return;
        }

        // Buscar si ya hay un ambientAudioSource
        ambientAudioSource = GameObject.Find("AmbientAudioSource")?.GetComponent<AudioSource>();

        if (ambientAudioSource == null)
        {
            // Si no existe, crearlo
            GameObject ambientAudioObject = new GameObject("AmbientAudioSource");
            ambientAudioSource = ambientAudioObject.AddComponent<AudioSource>();
        }

        // Configurar como audio 2D
        ambientAudioSource.spatialBlend = 0f;
        ambientAudioSource.clip = ambientSound;
        ambientAudioSource.loop = true;
        ambientAudioSource.volume = 0.5f;
        ambientAudioSource.playOnAwake = false;

        if (!ambientAudioSource.isPlaying)
        {
            ambientAudioSource.Play();
            Debug.Log("Reproduciendo ambientSound: " + ambientSound.name);
        }
        else
        {
            Debug.Log("ambientAudioSource ya estaba reproduciendo");
        }

        Debug.Log("ambientAudioSource.isPlaying: " + ambientAudioSource.isPlaying);
    }
}
