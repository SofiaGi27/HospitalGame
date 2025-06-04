using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager Singleton creado y marcado como DontDestroyOnLoad");

            // Registrarse para escuchar eventos de cambio de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (Instance != this)
        {
            Debug.Log("Se destruye una instancia duplicada de GameManager");
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        // Dejar de escuchar eventos de cambio de escena
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Este método se llama cada vez que se carga una escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Escena cargada: {scene.name}");

        // Verificar si hay una bandera de respuesta incorrecta cuando regresamos a la escena Game
        if (scene.name == "Game")
        {
            CheckRespuestaIncorrecta();
        }
    }

    // Verificar si hay una respuesta incorrecta registrada y procesarla
    private void CheckRespuestaIncorrecta()
    {
        if (PlayerPrefs.GetInt("RespuestaIncorrecta", 0) == 1)
        {
            Debug.Log("Se detect� una respuesta incorrecta");

            // Intentar acceder al VidasManager
            if (VidasManager.Instance != null)
            {
                Debug.Log("Comunicando al VidasManager que debe quitar una vida");
                VidasManager.Instance.QuitarVida();

                // Reiniciar la bandera
                PlayerPrefs.SetInt("RespuestaIncorrecta", 0);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogWarning("No se encontr� una instancia de VidasManager para procesar la respuesta incorrecta");
            }
        }
    }

    // Método para iniciar el flujo del quiz
    public void IniciarQuiz()
    {
        SceneManager.LoadScene("Quiz");
    }

    // Método para reiniciar el juego
    public void ReiniciarJuego()
    {
        // Reiniciar las vidas
        if (VidasManager.Instance != null)
        {
            VidasManager.Instance.ReiniciarVidas();
        }

        // Cargar la escena inicial
        SceneManager.LoadScene("Menu");
    }

}