using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Texture2D backgroundTexture; 

    [SerializeField] private AudioClip sonidoAmbiente;
    [SerializeField] private AudioClip sonidoClick;
    [SerializeField] private AudioClip sonidoHover;

    private AudioSource audioSource;
    private Button playButton;

    private void OnEnable()
    {

        //AudioSource 
    audioSource = GetComponent<AudioSource>();
    if (audioSource == null)
    {
    audioSource = gameObject.AddComponent<AudioSource>();
    }

    // Sonido ambiente
    if (sonidoAmbiente != null)
    {
    audioSource.clip = sonidoAmbiente;
    audioSource.loop = true;
    audioSource.Play();
    }

          
        var root = uiDocument.rootVisualElement;

        
        root.style.backgroundImage = new StyleBackground(backgroundTexture);

        // Obtiene referencia al botón
        playButton = root.Q<Button>("PlayButton");

        
        playButton.RegisterCallback<ClickEvent>(_ =>
        {
        ReproducirSonidoClick();
        StartGame();
        });

        playButton.RegisterCallback<MouseEnterEvent>(_ => OnButtonHover(true));
        playButton.RegisterCallback<MouseLeaveEvent>(_ => OnButtonHover(false));
    }

    private void StartGame()
    {
        Debug.Log("Escena actual: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        Debug.Log("Personaje seleccionado (PlayerPrefs): " + PlayerPrefs.GetInt("SelectedCharacterIndex", -1));
        Debug.Log("¡Iniciando juego!");
        // Cargar la escena del juego
        SceneManager.LoadScene("LoginScene");
    }

    private void OnButtonHover(bool isHovering)
    {
    if (isHovering)
    {
        playButton.AddToClassList("button-hover");
        ReproducirSonidoHover();
    }
    else
    {
        playButton.RemoveFromClassList("button-hover");
    }
}


    private void ReproducirSonidoClick()
    {
        if (audioSource != null && sonidoClick != null)
        {
            audioSource.PlayOneShot(sonidoClick);
        }
    }
    
    private void ReproducirSonidoHover()
    {
    if (audioSource != null && sonidoHover != null)
    {
        audioSource.PlayOneShot(sonidoHover);
    }
    }

}