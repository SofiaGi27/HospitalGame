using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterClickHandler : MonoBehaviour
{
    public int characterIndex;

    void OnMouseDown()
    {
        Debug.Log($"[CharacterClickHandler] Clic detectado en: {gameObject.name}");

        // Guardar índice seleccionado
        PlayerPrefs.SetInt("SelectedCharacterIndex", characterIndex);
        Debug.Log($"[CharacterClickHandler] Personaje seleccionado: {characterIndex}");

        // Intentar cargar la escena
        Debug.Log("[CharacterClickHandler] Intentando cargar escena 'Game'...");
        SceneManager.LoadScene("Game");
    }
}