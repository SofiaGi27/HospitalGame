using UnityEngine;
using UnityEngine.UIElements;

public class PortadaHospitalSounds : MonoBehaviour
{
    public AudioClip hoverSound;
    public AudioClip selectSound;
    public AudioClip ambientSound;

    private AudioSource audioSource;
    private bool hasHovered = false;

    void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (ambientSound != null)
        {
            audioSource.clip = ambientSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        var root = GetComponent<UIDocument>().rootVisualElement;
        var jugarButton = root.Q<Button>("Jugar");

        if (jugarButton != null)
        {
            jugarButton.RegisterCallback<PointerEnterEvent>(ev => {
                if (!hasHovered)
                {
                    PlayHoverSound();
                    hasHovered = true;
                }
            });

            jugarButton.RegisterCallback<PointerLeaveEvent>(ev => {
                hasHovered = false;
            });

            jugarButton.RegisterCallback<ClickEvent>(ev => PlaySelectSound());
        }
    }

    private void PlayHoverSound()
    {
        if (hoverSound != null)
        {
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(hoverSound);
        }
    }

    private void PlaySelectSound()
    {
        if (selectSound != null)
        {
            audioSource.pitch = 1f;
            audioSource.PlayOneShot(selectSound);
        }
    }
}