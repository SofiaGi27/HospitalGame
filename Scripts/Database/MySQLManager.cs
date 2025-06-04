using MySqlConnector;
using BCrypt.Net;
using UnityEngine;
using System;

public class MySQLManager : MonoBehaviour
{
    private string server = "localhost";
    private string database = "hospitalbbdd2";
    private string user = "pruebas";
    private string password = "xC5qpJL.YA8sUrbB";
    private string connectionString;
    private MySqlConnection connection;
    public static MySQLManager Instance { get; private set; }

    public EspecialidadService especialidadService { get; private set; }
    public PersonajeService personajeService { get; private set; }
    public PreguntaService preguntaService { get; private set; }
    public RespuestaService respuestaService { get; private set; }
    public UsuarioService usuarioService { get; private set; }
    public UsuarioPreguntaService usuarioPreguntaService { get; private set; }
    public EmailVerificationService emailVerificationService { get; private set; }

    public RolService rolService { get; private set; }
    public PuntajeService puntajeService { get; private set; }


    void Awake()
    {
        // Construimos connectionString en Awake para evitar errores de inicialización.
        connectionString = $"Server={server};Database={database};User={user};Password={password};";

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre escenas
        }
        else
        {
            Destroy(gameObject); // Evita duplicados
        }
    }

    void Start()
    {
        try
        {
            connection = new MySqlConnection(connectionString);
            connection.Open();
            Debug.Log("✅ Conectado a MySQL.");
            especialidadService = new EspecialidadService(connection);
            personajeService = new PersonajeService(connection);
            preguntaService = new PreguntaService(connection);
            respuestaService = new RespuestaService(connection);
            usuarioService = new UsuarioService(connection);
            usuarioPreguntaService = new UsuarioPreguntaService(connection);
            emailVerificationService = new EmailVerificationService(connection);    
            rolService = new RolService(connection);
            puntajeService = new PuntajeService(connection);
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al conectar con MySQL: " + e.Message);
        }
    }

    public MySqlConnection GetConnection()
    {
        return connection;
    }

    void OnApplicationQuit()
    {
        if (connection != null && connection.State == System.Data.ConnectionState.Open)
        {
            connection.Close();
            Debug.Log("🔌 Conexión cerrada.");
        }
    }

}
