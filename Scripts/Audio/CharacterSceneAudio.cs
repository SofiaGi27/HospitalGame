using UnityEngine;

public class CharacterSceneAudio : MonoBehaviour
{
    // Sonidos para la aparición del personaje y otros efectos
    public AudioClip characterAppearSound;  // Sonido que suena cuando el personaje aparece

    // Sonidos para la escena (ambiental y respiración)
    public AudioClip ambientSound;  // Sonido ambiental para la escena
    public AudioClip breathingSound;  // Sonido de respiración

    // Sonido de la ambulancia
    public AudioClip ambulanceSound;  // Sonido de la ambulancia

    private AudioSource audioSource;
    private AudioSource breathingAudioSource;

    void Start()
    {
        // Asegúrate de tener un AudioSource para ambos sonidos
        audioSource = GetComponent<AudioSource>();

        // Crear un AudioSource adicional para la respiración
        breathingAudioSource = gameObject.AddComponent<AudioSource>();  

        // Reproducir el sonido de aparición del personaje
        PlayCharacterAppearSound();

        // Reproducir el sonido ambiental en bucle
        PlayAmbientSound();

        // Reproducir el sonido de respiración en bucle
        PlayBreathingSound();

        // Reproducir el sonido de la ambulancia (puede ser en cualquier momento que desees)
        PlayAmbulanceSound();
    }

    private void PlayCharacterAppearSound()
    {
        // Verifica si se tiene el clip asignado y lo reproduce
        if (characterAppearSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(characterAppearSound);  // Reproduce solo una vez el sonido de aparición
        }
        else
        {
            Debug.LogWarning("¡Falta el clip de sonido de aparición o el AudioSource!");
        }
    }

    private void PlayAmbientSound()
    {
        if (ambientSound != null && audioSource != null)
        {
            audioSource.clip = ambientSound;
            audioSource.loop = true;  // Hace que el sonido ambiental se repita en bucle
            audioSource.volume = 0.5f;  // Ajusta el volumen del sonido ambiental si es necesario
            audioSource.Play();  // Reproduce el sonido ambiental
        }
        else
        {
            Debug.LogWarning("¡Falta el clip de sonido ambiental o el AudioSource!");
        }
    }

    private void PlayBreathingSound()
    {
        if (breathingSound != null && breathingAudioSource != null)
        {
            breathingAudioSource.clip = breathingSound;
            breathingAudioSource.loop = true;  // Hace que el sonido de respiración se repita en bucle
            breathingAudioSource.volume = 0.3f;  // Ajusta el volumen del sonido de respiración si es necesario
            breathingAudioSource.Play();  // Reproduce el sonido de respiración
        }
        else
        {
            Debug.LogWarning("¡Falta el clip de sonido de respiración o el AudioSource!");
        }
    }

    private void PlayAmbulanceSound()
    {
        if (ambulanceSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(ambulanceSound);  // Reproduce el sonido de la ambulancia una sola vez
        }
        else
        {
            Debug.LogWarning("¡Falta el clip de sonido de ambulancia o el AudioSource!");
        }
    }
}