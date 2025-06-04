using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Autodesk.Fbx;
using MySqlConnector;
using System.Collections;


public class JsonToDatabaseLoader : MonoBehaviour
{
    [Serializable]
    public class Opcion
    {
        public string texto;
        public bool correcta;
    }

    [Serializable]
    public class PreguntaJson
    {
        public string pregunta;
        public List<Opcion> opciones;
    }
    [SerializeField] private String rolEspecialidad = "";// A cambiar en el inspector
    [SerializeField] private int idEspecialidad = 1; // ID de la especialidad a la que pertenecen las preguntas
    [SerializeField] private TextAsset jsonFile; // Archivo JSON asignado desde el editor
    [SerializeField] private MySQLManager dbManager; // Referencia al gestor de base de datos

    // Configuración de validación
    [SerializeField] private int opcionesPorPregunta = 4; // Número exacto de opciones permitidas
    [SerializeField] private bool mostrarLogsDetallados = true; // Para habilitar/deshabilitar logs detallados

    private void Awake()
    {
        // Esperar un momento para asegurarse de que MySQLManager se inicialice primero
        StartCoroutine(IniciarConRetraso());
    }

    private IEnumerator IniciarConRetraso()
    {
        // Esperar para asegurar que MySQLManager se ha inicializado
        yield return new WaitForSeconds(0.5f);
        
    }
    private void Start()
    {
        Debug.Log("JsonToDatabaseLoader: Iniciando componente...");

        // Verificar referencias críticas
        if (dbManager == null)
        {
            Debug.LogError("JsonToDatabaseLoader: No se ha asignado el DBManager. Por favor, asignarlo en el Inspector.");
        }
        else
        {
            Debug.Log("JsonToDatabaseLoader: DBManager asignado correctamente.");

            // Verificar servicios
            if (dbManager.preguntaService == null)
            {
                Debug.LogError("JsonToDatabaseLoader: dbManager.preguntaService es null. Verifique que esté correctamente inicializado.");
            }

            if (dbManager.respuestaService == null)
            {
                Debug.LogError("JsonToDatabaseLoader: dbManager.respuestaService es null. Verifique que esté correctamente inicializado.");
            }
        }

        
        CargarPreguntasDesdeJSON();
    }

    // Estadísticas de carga
    private int preguntasExitosas = 0;
    private int preguntasFallidas = 0;
    private int respuestasExitosas = 0;
    private int respuestasFallidas = 0;
    private int preguntasInvalidas = 0;

    public void CargarPreguntasDesdeJSON()
    {
        try
        {
            // Verificar que la configuración es correcta
            Debug.Log("Iniciando carga de preguntas desde JSON...");

            // Verificar que tenemos dbManager
            if (dbManager == null)
            {
                Debug.LogError("No se ha asignado el DBManager");
                return;
            }

            // Verificar que tenemos los servicios necesarios
            if (dbManager.preguntaService == null)
            {
                Debug.LogError("dbManager.preguntaService es null");
                return;
            }

            if (dbManager.respuestaService == null)
            {
                Debug.LogError("dbManager.respuestaService es null");
                return;
            }

            // Si no hay archivo JSON asignado, mostrar error
            if (jsonFile == null)
            {
                Debug.LogError("No se ha asignado ningún archivo JSON");
                return;
            }

            string jsonContent = jsonFile.text;

            // Mostrar una pequeña muestra del JSON para verificación visual
            //Debug.Log($"Muestra del JSON: {jsonContent.Substring(0, Math.Min(200, jsonContent.Length))}...");

            // Reiniciar estadísticas
            preguntasExitosas = 0;
            preguntasFallidas = 0;
            respuestasExitosas = 0;
            respuestasFallidas = 0;
            preguntasInvalidas = 0;

            // Parsear el JSON que es un array de preguntas
            try
            {
                List<PreguntaJson> preguntas = JsonConvert.DeserializeObject<List<PreguntaJson>>(jsonContent);

                if (preguntas == null)
                {
                    Debug.LogError("Error al deserializar el JSON: El resultado es null");
                    return;
                }

                Debug.Log($"Se encontraron {preguntas.Count} preguntas en el JSON");

                if (preguntas.Count > 0)
                {
                    // Mostrar datos de la primera pregunta para verificación
                    var primera = preguntas[0];
                    Debug.Log($"Primera pregunta: '{primera.pregunta}'");
                    Debug.Log($"Número de opciones: {primera.opciones?.Count ?? 0}");

                    if (primera.opciones != null && primera.opciones.Count > 0)
                    {
                        for (int i = 0; i < primera.opciones.Count; i++)
                        {
                            Debug.Log($"  Opción {i + 1}: '{primera.opciones[i].texto}' (Correcta: {primera.opciones[i].correcta})");
                        }
                    }
                }

                ProcesarPreguntas(preguntas);

                Debug.Log($"Proceso completado: {preguntasExitosas} preguntas y {respuestasExitosas} respuestas insertadas correctamente. " +
                         $"Fallidas: {preguntasFallidas} preguntas y {respuestasFallidas} respuestas. " +
                         $"Preguntas inválidas (omitidas): {preguntasInvalidas}");
            }
            catch (JsonReaderException jsonEx)
            {
                Debug.LogError($"Error en el formato del JSON: {jsonEx.Message}\nRuta: {jsonEx.Path}\nLínea: {jsonEx.LineNumber}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al cargar las preguntas: {ex.Message}\nStack trace: {ex.StackTrace}");
        }
    }

    private void ProcesarPreguntas(List<PreguntaJson> preguntas)
    {
        if (preguntas == null || preguntas.Count == 0)
        {
            Debug.LogWarning("No se encontraron preguntas en el JSON");
            return;
        }

        Debug.Log($"Comenzando a procesar {preguntas.Count} preguntas...");

        // Procesar cada pregunta individualmente
        foreach (var preguntaJson in preguntas)
        {
            // Validar la pregunta antes de procesarla
            if (!ValidarPregunta(preguntaJson))
            {
                preguntasInvalidas++;
                continue;
            }

            // Insertar la pregunta y obtener su ID
            int idPregunta = InsertarPreguntaYObtenerID(preguntaJson.pregunta);

            if (idPregunta > 0)
            {
                // Si la pregunta se insertó correctamente, procesar sus opciones
                foreach (var opcion in preguntaJson.opciones)
                {
                    InsertarRespuesta(opcion.texto, opcion.correcta, idPregunta);
                }
            }
        }
    }

    private bool ValidarPregunta(PreguntaJson pregunta)
    {
        if (string.IsNullOrWhiteSpace(pregunta.pregunta))
        {
            Debug.LogWarning("Se encontró una pregunta con texto vacío");
            return false;
        }

        if (pregunta.opciones == null || pregunta.opciones.Count == 0)
        {
            Debug.LogWarning($"La pregunta '{TruncateText(pregunta.pregunta, 40)}' no tiene opciones");
            return false;
        }

        if (pregunta.opciones.Count != opcionesPorPregunta)
        {
            Debug.LogWarning($"La pregunta '{TruncateText(pregunta.pregunta, 40)}' tiene {pregunta.opciones.Count} opciones, pero se requieren {opcionesPorPregunta}");
            return false;
        }

        int opcionesCorrectas = pregunta.opciones.Count(o => o.correcta);
        if (opcionesCorrectas != 1)
        {
            Debug.LogWarning($"La pregunta '{TruncateText(pregunta.pregunta, 40)}' tiene {opcionesCorrectas} opciones correctas, pero debe tener exactamente 1");
            return false;
        }

        // Verificar que todas las opciones tengan texto
        foreach (var opcion in pregunta.opciones)
        {
            if (string.IsNullOrWhiteSpace(opcion.texto))
            {
                Debug.LogWarning($"La pregunta '{TruncateText(pregunta.pregunta, 40)}' tiene una opción con texto vacío");
                return false;
            }
        }

        return true;
    }

    private int InsertarPreguntaYObtenerID(string textoPregunta)
    {
        try
        {
            // Validar que dbManager no sea nulo
            if (dbManager == null)
            {
                Debug.LogError("Error: dbManager es null");
                preguntasFallidas++;
                return -1;
            }

            // Validar que preguntaService no sea nulo
            if (dbManager.preguntaService == null)
            {
                Debug.LogError("Error: dbManager.preguntaService es null");
                preguntasFallidas++;
                return -1;
            }

            // Insertar la pregunta en la base de datos
            Debug.Log($"Intentando insertar pregunta: '{TruncateText(textoPregunta, 40)}' con especialidad ID: {idEspecialidad}");

            bool exito = dbManager.preguntaService.Crear(idEspecialidad, textoPregunta, rolEspecialidad);

            if (exito)
            {
                preguntasExitosas++;

                // Obtener el último ID insertado usando LAST_INSERT_ID() de MySQL
                int ultimoId = ObtenerUltimoIdInsertado();

                if (ultimoId > 0)
                {
                    if (mostrarLogsDetallados)
                    {
                        Debug.Log($"Pregunta insertada con ID {ultimoId}: '{TruncateText(textoPregunta, 40)}'");
                    }
                    return ultimoId;
                }
                else
                {
                    Debug.LogWarning($"No se pudo obtener el ID de la pregunta insertada: {TruncateText(textoPregunta, 40)}");
                    return -1;
                }
            }
            else
            {
                preguntasFallidas++;
                Debug.LogWarning($"No se pudo insertar la pregunta: {TruncateText(textoPregunta, 40)}");
                return -1;
            }
        }
        catch (Exception ex)
        {
            preguntasFallidas++;
            Debug.LogError($"Error al insertar pregunta: {ex.Message}\nStack trace: {ex.StackTrace}");
            return -1;
        }
    }

    // Método para obtener el último ID insertado en la base de datos
    private int ObtenerUltimoIdInsertado()
    {
        try
        {
            // Validaciones para evitar NullReferenceException
            if (dbManager == null)
            {
                Debug.LogError("Error: dbManager es null en ObtenerUltimoIdInsertado");
                return -1;
            }

            if (dbManager.preguntaService == null)
            {
                Debug.LogError("Error: dbManager.preguntaService es null en ObtenerUltimoIdInsertado");
                return -1;
            }

            int resultado = dbManager.preguntaService.ObtenerUltimoIdInsertado();

            Debug.Log($"Último ID obtenido: {resultado}");

            return resultado;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener último ID insertado: {ex.Message}\nStack trace: {ex.StackTrace}");
            return -1;
        }
    }

    private bool InsertarRespuesta(string texto, bool esCorrecta, int idPregunta)
    {
        try
        {
            // Validaciones para evitar NullReferenceException
            if (dbManager == null)
            {
                Debug.LogError("Error: dbManager es null en InsertarRespuesta");
                respuestasFallidas++;
                return false;
            }

            if (dbManager.respuestaService == null)
            {
                Debug.LogError("Error: dbManager.respuestaService es null en InsertarRespuesta");
                respuestasFallidas++;
                return false;
            }

            // Log para verificar si el valor de esCorrecta está llegando correctamente
            if (mostrarLogsDetallados)
            {
                Debug.Log($"Insertando respuesta para pregunta {idPregunta}: '{TruncateText(texto, 30)}' (¿Es correcta? {esCorrecta})");
            }

            bool exito = dbManager.respuestaService.Crear(idPregunta, texto, esCorrecta);

            if (exito)
            {
                respuestasExitosas++;
                if (mostrarLogsDetallados)
                {
                    Debug.Log($"Respuesta insertada correctamente para pregunta {idPregunta}: '{TruncateText(texto, 30)}' (Correcta: {esCorrecta})");
                }
                return true;
            }
            else
            {
                respuestasFallidas++;
                Debug.LogWarning($"No se pudo insertar la respuesta para pregunta {idPregunta}: {TruncateText(texto, 30)}");
                return false;
            }
        }
        catch (Exception ex)
        {
            respuestasFallidas++;
            Debug.LogError($"Error al insertar respuesta: {ex.Message}\nStack trace: {ex.StackTrace}");
            return false;
        }
    }

    // Helper para truncar texto largo en logs
    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return (text.Length <= maxLength) ? text : text.Substring(0, maxLength) + "...";
    }

}