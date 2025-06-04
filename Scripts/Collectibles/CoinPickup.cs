using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public CoinSystem coinSystem; 
    private AudioSource audioSource;
    private Collider coinCollider;
    private Renderer coinRenderer;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        coinCollider = GetComponent<Collider>();
        coinRenderer = GetComponent<Renderer>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (coinSystem != null)
            {
                coinSystem.CollectCoin();
            }

            // Ocultar visualmente y desactivar colisión
            if (coinRenderer != null) coinRenderer.enabled = false;
            if (coinCollider != null) coinCollider.enabled = false;

            // Reproducir sonido y destruir el objeto al finalizar
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
                Destroy(gameObject, audioSource.clip.length);
            }
            else
            {
                // Si no hay sonido, destruir de inmediato
                Destroy(gameObject);
            }
        }
    }
}
