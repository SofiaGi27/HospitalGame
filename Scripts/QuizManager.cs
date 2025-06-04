using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    // Referencia al documento UI
    [SerializeField] private UIDocument quizDocument;

    // Referencias a elementos UI
    private Label questionText;
    private VisualElement answersContainer;
    private Button[] answerButtons = new Button[4];
    private Label[] answerTexts = new Label[4];

    // Elementos UI para mostrar progreso y puntaje
    private Label progresoLabel;
    private Label puntajeLabel;

    // Variables para control del quiz
    private List<Pregunta> todasLasPreguntas = new List<Pregunta>();
    private List<Pregunta> preguntasDisponibles = new List<Pregunta>();
    private Pregunta preguntaActual = null;
    private List<Respuesta> respuestasActuales = new List<Respuesta>();
    private bool esperandoProximaPregunta = false;

    // Variables para el sistema de 10 preguntas y puntaje
    private int preguntaActualIndex = 0;
    private const int TOTAL_PREGUNTAS = 10;
    private int puntajeTotal = 0;
    private int puntajePorCorrecta = 100;
    private int puntajePorIncorrecta = -30;
    private bool puntajeYaGuardado = false;

    // Configuración para las pantallas de resultado
    [Header("Configuración de Pantallas de Resultado")]
    [SerializeField] private int puntajeMinimoParaAprobar = 600;
    [SerializeField] private string escenaCongratulations = "CongratulationsScreen";
    [SerializeField] private string escenaLevelFailed = "LevelFailed";
    [SerializeField] private float tiempoEsperaAntesDelCambio = 3.0f;

    // Lista de IDs de preguntas ya completadas por el usuario
    private HashSet<int> preguntasCompletadas = new HashSet<int>();

    // ID del usuario y rol actual
    int idUsuario = UserSession.Instance.IdUsuario;
    int rolUsuario = UserSession.Instance.RolUsuario;
    int especialidadActual = UserSession.Instance.EspecialidadActual;

    // Parámetros configurables
    [SerializeField] private bool eliminarPreguntasContestadas = true;
    [SerializeField] private bool barajarPreguntas = true;
    [SerializeField] private bool barajarRespuestas = true;
    [SerializeField] private float tiempoTransicionEntrePreguntas = 1.0f;

    // Referencias para audio 
    [SerializeField] private AudioClip sonidoCorrecta;
    [SerializeField] private AudioClip sonidoIncorrecta;
    [SerializeField] private AudioClip sonidoAmbiente;
    [SerializeField] private AudioClip sonidoHover;
    private AudioSource audioSource;

    private void Awake()
    {
        // Inicializar audio source si es necesario
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (sonidoCorrecta != null || sonidoIncorrecta != null || sonidoAmbiente != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (sonidoAmbiente != null)
        {
            audioSource.clip = sonidoAmbiente;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void Start()
    {
        // Esperar un momento para asegurarse de que MySQLManager se inicialice primero
        StartCoroutine(IniciarConRetraso());
    }

    private IEnumerator IniciarConRetraso()
    {
        yield return new WaitForSeconds(0.5f);
        InitializeUI();

        // Primero cargar las preguntas ya completadas del usuario
        CargarPreguntasCompletadas();
    }

    private void InitializeUI()
    {
        try
        {
            if (quizDocument == null)
            {
                quizDocument = GetComponent<UIDocument>();
                if (quizDocument == null)
                {
                    Debug.LogError("No se ha encontrado el UIDocument");
                    return;
                }
            }

            var root = quizDocument.rootVisualElement;

            questionText = root.Q<Label>("question-text");
            answersContainer = root.Q<VisualElement>("answers-container");

            // Obtener elementos UI para progreso y puntaje (opcional)
            progresoLabel = root.Q<Label>("progreso-label");
            puntajeLabel = root.Q<Label>("puntaje-label");

            for (int i = 0; i < 4; i++)
            {
                answerButtons[i] = root.Q<Button>($"answer-button-{i}");
                answerTexts[i] = root.Q<Label>($"answer-text-{i}");

                if (answerButtons[i] != null)
                {
                    int index = i;
                    answerButtons[i].clickable.clicked += () => VerificarRespuestaPorIndice(index);

                    //Evento cuando el mouse pasa sobre el botón (hover)
                    answerButtons[i].RegisterCallback<MouseEnterEvent>((evt) => ReproducirSonidoHover());
                }
                else
                {
                    Debug.LogWarning($"No se encontró el botón de respuesta {i}");
                }
            }

            // Inicializar display de progreso y puntaje
            ActualizarUI();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al inicializar UI: {ex.Message}");
        }
    }

    /// Busca las preguntas que el usuario ya ha contestado correctamente
    private async void CargarPreguntasCompletadas()
    {
        try
        {
            ElegirEspecialidad(rolUsuario);
            if (MySQLManager.Instance == null || idUsuario <= 0)
            {
                Debug.LogWarning("No se puede cargar historial: MySQLManager no disponible o usuario no identificado");
                CargarPreguntas(); // Continuar de todos modos con todas las preguntas
                return;
            }

            // Aquí deberías tener un método en tu servicio para obtener las preguntas ya completadas
            var preguntasYaCompletadas = await MySQLManager.Instance.usuarioPreguntaService.ObtenerPreguntasCompletadasPorUsuario(idUsuario);

            // Guardar los IDs de preguntas completadas en el HashSet
            preguntasCompletadas.Clear();
            foreach (var pregunta in preguntasYaCompletadas)
            {
                preguntasCompletadas.Add(pregunta.Id);
            }

            Debug.Log($"Cargadas {preguntasCompletadas.Count} preguntas ya completadas por el usuario");

            // Ahora cargar todas las preguntas y filtrar
            CargarPreguntas();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al cargar preguntas completadas: {ex.Message}");
            CargarPreguntas(); // Continuar de todos modos
        }
    }

    ///Busca todas las preguntas segun la especialidadActual y excluye las que ya han sido contestadas
    private async void CargarPreguntas()
    {
        try
        {
            ElegirEspecialidad(rolUsuario);
            if (MySQLManager.Instance == null || MySQLManager.Instance.preguntaService == null)
            {
                Debug.LogError("Error al cargar preguntas: Servicio no disponible");
                return;
            }

            todasLasPreguntas = especialidadActual > 0
                ? await MySQLManager.Instance.preguntaService.ObtenerPreguntasPorEspecialidad(especialidadActual)
                : await MySQLManager.Instance.preguntaService.ObtenerTodasLasPreguntas();

            // Filtrar las preguntas para excluir las ya completadas
            todasLasPreguntas = todasLasPreguntas.Where(p => !preguntasCompletadas.Contains(p.Id)).ToList();

            Debug.Log($"Cargadas {todasLasPreguntas.Count} preguntas disponibles después de filtrar de la especialidad: {especialidadActual}");

            // Verificar que hay suficientes preguntas
            if (todasLasPreguntas.Count < TOTAL_PREGUNTAS)
            {
                Debug.LogWarning($"Solo hay {todasLasPreguntas.Count} preguntas disponibles, menos de las {TOTAL_PREGUNTAS} requeridas");
            }

            ReiniciarPreguntasDisponibles();

            // Inicializar el contador y mostrar la primera pregunta
            preguntaActualIndex = 0;
            puntajeTotal = 0;
            ActualizarUI();
            MostrarSiguientePregunta();

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al cargar preguntas: {ex.Message}");
        }
    }

    public void ReiniciarPreguntasDisponibles()
    {
        preguntasDisponibles = new List<Pregunta>(todasLasPreguntas);
        if (barajarPreguntas) preguntasDisponibles = preguntasDisponibles.OrderBy(p => UnityEngine.Random.value).ToList();
    }

    public void MostrarSiguientePregunta()
    {
        // Resetear el estado de espera
        esperandoProximaPregunta = false;

        // Verificar si ya se completaron las 10 preguntas
        if (preguntaActualIndex >= TOTAL_PREGUNTAS)
        {
            FinalizarQuiz();
            return;
        }

        // Verificar si hay preguntas disponibles
        if (preguntasDisponibles.Count == 0)
        {
            Debug.LogWarning("No hay más preguntas disponibles");
            FinalizarQuiz();
            return;
        }

        // Obtener la siguiente pregunta
        preguntaActual = preguntasDisponibles[0];

        // Si está configurado para eliminar preguntas contestadas, quitarla de la lista
        if (eliminarPreguntasContestadas)
        {
            preguntasDisponibles.RemoveAt(0);
        }

        // Mostrar el texto de la pregunta
        if (questionText != null)
        {
            questionText.text = preguntaActual.TextoPregunta;
        }

        // Actualizar la UI con el progreso actual
        ActualizarUI();

        // Cargar las respuestas para esta pregunta
        CargarRespuestasParaPregunta(preguntaActual.Id);
    }

    private void FinalizarQuiz()
    {
        if (questionText != null)
        {
            questionText.text = $"¡Quiz completado!\nPuntaje final: {puntajeTotal} puntos";
        }

        foreach (var button in answerButtons)
        {
            if (button != null) button.SetEnabled(false);
        }

        Debug.Log($"Quiz finalizado. Puntaje total: {puntajeTotal}");
        IntentarGuardarPuntaje();

        // Mostrar la pantalla apropiada según el puntaje
        MostrarPantallaResultado();
    }

    /// <summary>
    /// Muestra la pantalla de resultado apropiada según el puntaje obtenido
    /// </summary>
    private void MostrarPantallaResultado()
    {
        if (puntajeTotal >= puntajeMinimoParaAprobar)
        {
            Debug.Log($"¡Felicitaciones! Puntaje suficiente ({puntajeTotal} >= {puntajeMinimoParaAprobar}). Mostrando pantalla de congratulaciones.");
            StartCoroutine(CambiarAEscenaConEspecialidad(tiempoEsperaAntesDelCambio, escenaCongratulations, true));
        }
        else
        {
            Debug.Log($"Puntaje insuficiente ({puntajeTotal} < {puntajeMinimoParaAprobar}). Mostrando pantalla de nivel fallido.");
            StartCoroutine(CambiarAEscenaConEspecialidad(tiempoEsperaAntesDelCambio, escenaLevelFailed, false));
        }
    }

    /// <summary>
    /// Cambia a la escena de resultado y configura PlayerPrefs para que la pantalla sepa qué especialidad mostrar
    /// </summary>
    private IEnumerator CambiarAEscenaConEspecialidad(float delay, string nombreEscena, bool esVictoria)
    {
        yield return new WaitForSeconds(delay);

        // Guardar información para que las pantallas puedan usarla
        PlayerPrefs.SetInt("EspecialidadCompletada", especialidadActual);
        PlayerPrefs.SetInt("PuntajeFinal", puntajeTotal);
        PlayerPrefs.SetInt("EsVictoria", esVictoria ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"Cambiando a escena: {nombreEscena} con especialidad: {especialidadActual}");
        SceneManager.LoadScene(nombreEscena);
    }

    private void ActualizarUI()
    {
        // Actualizar label de progreso a implementar
        if (progresoLabel != null)
        {
            progresoLabel.text = $"Pregunta {preguntaActualIndex + 1} de {TOTAL_PREGUNTAS}";
        }

        // Actualizar label de puntaje 
        if (puntajeLabel != null)
        {
            puntajeLabel.text = $"Puntaje: {puntajeTotal}";
        }
    }

    private async void CargarRespuestasParaPregunta(int idPregunta)
    {
        try
        {
            respuestasActuales = await MySQLManager.Instance.respuestaService.ObtenerRespuestasPorPreguntaId(idPregunta);
            MostrarRespuestasEnUI(respuestasActuales);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al cargar respuestas: {ex.Message}");
        }
    }

    private void MostrarRespuestasEnUI(List<Respuesta> respuestas)
    {
        List<Respuesta> respuestasAMostrar = new List<Respuesta>(respuestas);
        if (barajarRespuestas) respuestasAMostrar = respuestasAMostrar.OrderBy(r => UnityEngine.Random.value).ToList();

        ///Para ver la respuesta en la consola
        for (int i = 0; i < respuestasAMostrar.Count; i++)
        {
            if (respuestasAMostrar[i].EsCorrecta)
            {
                String respuesta = respuestasAMostrar[i].TextoRespuesta;
                Debug.Log($"La respuesta es: {respuesta}");
            }
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < respuestasAMostrar.Count)
            {
                answerButtons[i].style.display = DisplayStyle.Flex;
                answerButtons[i].SetEnabled(true);
                answerButtons[i].RemoveFromClassList("correct");
                answerButtons[i].RemoveFromClassList("incorrect");
                answerTexts[i].text = respuestasAMostrar[i].TextoRespuesta;
                answerButtons[i].userData = respuestasAMostrar[i];
            }
            else
            {
                answerButtons[i].style.display = DisplayStyle.None;
            }
        }
    }

    public void VerificarRespuestaPorIndice(int indice)
    {
        // Evitar múltiples clicks mientras se procesa una respuesta
        if (esperandoProximaPregunta) return;

        if (indice < 0 || indice >= answerButtons.Length || answerButtons[indice] == null) return;

        if (answerButtons[indice].userData is Respuesta respuesta)
        {
            // Marcar que estamos procesando una respuesta
            esperandoProximaPregunta = true;

            // Deshabilitar todos los botones temporalmente
            foreach (var button in answerButtons) if (button != null) button.SetEnabled(false);

            OnAnswerSelected(answerButtons[indice], respuesta.EsCorrecta);
            ProcesarRespuesta(respuesta);
        }
    }

    private void ProcesarRespuesta(Respuesta respuesta)
    {
        if (respuesta.EsCorrecta)
        {
            Debug.Log("Respuesta correcta");

            // Sumar puntos por respuesta correcta
            puntajeTotal += puntajePorCorrecta;
            Debug.Log($"Puntaje actual: {puntajeTotal} (+{puntajePorCorrecta})");

            // Guardar la pregunta como completada en la base de datos
            if (idUsuario > 0 && preguntaActual != null)
            {
                GuardarPreguntaCompletada(preguntaActual.Id);
            }

            if (audioSource != null && sonidoCorrecta != null)
                audioSource.PlayOneShot(sonidoCorrecta);
        }
        else
        {
            Debug.Log("Respuesta incorrecta");

            // Restar puntos por respuesta incorrecta
            puntajeTotal += puntajePorIncorrecta;
            Debug.Log($"Puntaje actual: {puntajeTotal} ({puntajePorIncorrecta})");

            // Intentar actualizar directamente el VidasManager si está disponible en esta escena
            if (VidasManager.Instance != null)
            {
                Debug.Log("Quitando vida a través del VidasManager.Instance");
                VidasManager.Instance.QuitarVida();
            }
            else
            {
                // Guardar una bandera para que GameManager procese la respuesta incorrecta
                Debug.Log("VidasManager.Instance no disponible, usando PlayerPrefs");
                PlayerPrefs.SetInt("RespuestaIncorrecta", 1);
                PlayerPrefs.Save();
            }

            if (audioSource != null && sonidoIncorrecta != null)
                audioSource.PlayOneShot(sonidoIncorrecta);
        }

        // Incrementar el contador de preguntas
        preguntaActualIndex++;

        // Actualizar la UI con el nuevo puntaje
        ActualizarUI();

        // Continuar con la siguiente pregunta después del delay
        StartCoroutine(MostrarSiguientePreguntaDespuesDeDelay(tiempoTransicionEntrePreguntas));
    }

    private async void GuardarPreguntaCompletada(int idPregunta)
    {
        try
        {
            if (MySQLManager.Instance == null || MySQLManager.Instance.usuarioPreguntaService == null || idUsuario <= 0)
            {
                Debug.LogWarning("No se puede guardar progreso: Servicio no disponible o usuario no identificado");
                return;
            }

            // Comprobar si esta pregunta ya se había completado previamente
            bool yaCompletada = await MySQLManager.Instance.usuarioPreguntaService.YaCompletoLaPreguntaAsync(idUsuario, idPregunta);
            if (yaCompletada)
            {
                Debug.Log($"La pregunta {idPregunta} ya estaba completada por el usuario {idUsuario}. No se guardará nuevamente.");
                return;
            }

            // Llamar al método para guardar la pregunta completada
            await MySQLManager.Instance.usuarioPreguntaService.GuardarPreguntaCompletada(idUsuario, idPregunta);

            // Actualizar la lista local de preguntas completadas
            preguntasCompletadas.Add(idPregunta);

            Debug.Log($"Pregunta {idPregunta} guardada como completada para el usuario {idUsuario}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al guardar pregunta completada: {ex.Message}");
        }
    }

    private async void GuardarPuntajeFinal(int puntajeFinal)
    {
        try
        {
            await MySQLManager.Instance.puntajeService.Crear(puntajeFinal, idUsuario, especialidadActual);
            Debug.Log($"Puntaje final guardado: {puntajeFinal}");

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al guardar puntaje final: {ex.Message}");
        }
    }

    private IEnumerator MostrarSiguientePreguntaDespuesDeDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Habilitar todos los botones para la siguiente pregunta
        foreach (var button in answerButtons)
        {
            if (button != null)
            {
                button.RemoveFromClassList("correct");
                button.RemoveFromClassList("incorrect");
                button.SetEnabled(true);
            }
        }

        MostrarSiguientePregunta();
    }

    private IEnumerator CambiarEscena(float delay, String escena)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(escena);
    }

    void OnAnswerSelected(Button answerButton, bool isCorrect)
    {
        answerButton.RemoveFromClassList("correct");
        answerButton.RemoveFromClassList("incorrect");
        answerButton.AddToClassList(isCorrect ? "correct" : "incorrect");

        if (!isCorrect) MostrarRespuestaCorrecta();
    }

    private void MostrarRespuestaCorrecta()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null && answerButtons[i].userData is Respuesta respuesta && respuesta.EsCorrecta)
            {
                answerButtons[i].RemoveFromClassList("incorrect");
                answerButtons[i].AddToClassList("correct");
                break;
            }
        }
    }

    public void CambiarEspecialidad(int idEspecialidad)
    {
        especialidadActual = idEspecialidad;
        CargarPreguntasCompletadas(); // Recargamos todo el proceso para filtrar por la nueva especialidad
    }

    public int ElegirEspecialidad(int rol)
    {
        switch (rol)
        {
            case 2:
                especialidadActual = 4;
                break;
            case 3:
                especialidadActual = 5;
                break;
        }
        return especialidadActual;
    }

    private void ReproducirSonidoHover()
    {
        if (audioSource != null && sonidoHover != null)
        {
            audioSource.PlayOneShot(sonidoHover);
        }
    }

    // Métodos públicos para configurar el puntaje 
    public void ConfigurarPuntajes(int puntajeCorrecta, int puntajeIncorrecta)
    {
        puntajePorCorrecta = puntajeCorrecta;
        puntajePorIncorrecta = puntajeIncorrecta;
    }

    public int ObtenerPuntajeActual()
    {
        return puntajeTotal;
    }

    public int ObtenerPreguntaActual()
    {
        return preguntaActualIndex + 1;
    }

    /// <summary>
    /// Método llamado cuando el jugador se queda sin vidas (Game Over)
    /// </summary>
    public void OnJugadorSinVidas()
    {
        Debug.Log("El jugador se quedó sin vidas, finalizando quiz...");

        // Guardar el puntaje actual
        IntentarGuardarPuntaje();

        // Deshabilitar todos los botones
        foreach (var button in answerButtons)
        {
            if (button != null) button.SetEnabled(false);
        }

        // Actualizar el texto de la pregunta para mostrar Game Over
        if (questionText != null)
        {
            questionText.text = $"¡Game Over!\nPuntaje final: {puntajeTotal} puntos";
        }

        // Mostrar la pantalla de resultado apropiada después de un breve delay
        StartCoroutine(MostrarResultadoDespuesDeGameOver());
    }

    /// <summary>
    /// Corrutina para mostrar el resultado después de Game Over
    /// </summary>
    private IEnumerator MostrarResultadoDespuesDeGameOver()
    {
        yield return new WaitForSeconds(2.0f); // Breve pausa para que el jugador vea el mensaje de Game Over

        Debug.Log("Mostrando resultado después de Game Over");
        MostrarPantallaResultado();
    }

    private void IntentarGuardarPuntaje()
    {
        if (puntajeYaGuardado)
        {
            Debug.Log("El puntaje ya fue guardado previamente. No se volverá a guardar.");
            return;
        }

        GuardarPuntajeFinal(puntajeTotal);
        puntajeYaGuardado = true;
    }

    #region Métodos públicos para configuración

    /// <summary>
    /// Configura el puntaje mínimo necesario para aprobar
    /// </summary>
    public void ConfigurarPuntajeMinimo(int puntajeMinimo)
    {
        puntajeMinimoParaAprobar = puntajeMinimo;
    }

    /// <summary>
    /// Configura las escenas de resultado
    /// </summary>
    public void ConfigurarEscenasResultado(string escenaVictoria, string escenaDerrota)
    {
        escenaCongratulations = escenaVictoria;
        escenaLevelFailed = escenaDerrota;
    }

    /// <summary>
    /// Obtiene la especialidad actual
    /// </summary>
    public int ObtenerEspecialidadActual()
    {
        return especialidadActual;
    }

    #endregion
}