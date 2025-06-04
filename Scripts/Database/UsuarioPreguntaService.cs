using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MySqlConnector;

public class UsuarioPreguntaService
{
    private MySqlConnection _conn;

    public UsuarioPreguntaService(MySqlConnection connection)
    {
        _conn = connection;
    }


    /// Guarda en la base de datos una pregunta completada por un usuario
    public async Task GuardarPreguntaCompletada(int idUsuario, int idPregunta)
    {
        if (_conn.State != System.Data.ConnectionState.Open)
            await _conn.OpenAsync();

        try
        {
            // Verificar si ya existe el registro 
            string checkQuery = "SELECT COUNT(*) FROM usuario_pregunta WHERE id_usuario = @idUsuario AND id_pregunta = @idPregunta";
            MySqlCommand checkCommand = new MySqlCommand(checkQuery, _conn);
            checkCommand.Parameters.AddWithValue("@idUsuario", idUsuario);
            checkCommand.Parameters.AddWithValue("@idPregunta", idPregunta);

            int count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

            if (count == 0)
            {
                // Insertar nuevo registro 
                MySqlCommand insertCommand = new MySqlCommand(
                    "INSERT INTO usuario_pregunta (id_usuario, id_pregunta) VALUES (@u, @p)",
                    _conn
                );
                insertCommand.Parameters.AddWithValue("@u", idUsuario);
                insertCommand.Parameters.AddWithValue("@p", idPregunta);

                await insertCommand.ExecuteNonQueryAsync();
                Debug.Log($"Registro agregado: usuario {idUsuario}, pregunta {idPregunta}");
            }
            else
            {
                Debug.Log($"El registro ya existe: usuario {idUsuario}, pregunta {idPregunta}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al guardar: {ex.Message}");
            throw;
        }
    }

    /// Obtiene todas las preguntas que un usuario ha completado correctamente
    public async Task<List<Pregunta>> ObtenerPreguntasCompletadasPorUsuario(int idUsuario)
    {
        List<Pregunta> preguntasCompletadas = new List<Pregunta>();

        if (_conn.State != System.Data.ConnectionState.Open)
            await _conn.OpenAsync();

        try
        {
            string query = @"
                SELECT p.* 
                FROM pregunta p
                INNER JOIN usuario_pregunta up ON p.id_pregunta = up.id_pregunta
                WHERE up.id_usuario = @idUsuario ";

            MySqlCommand command = new MySqlCommand(query, _conn);
            command.Parameters.AddWithValue("@idUsuario", idUsuario);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Pregunta pregunta = new Pregunta
                    {
                        Id = reader.GetInt32("id_pregunta"),
                        TextoPregunta = reader.GetString("pregunta"),
                        IdEspecialidad = reader.GetInt32("id_especialidad"),
                    };

                    preguntasCompletadas.Add(pregunta);
                }
            }

            Debug.Log($"Se encontraron {preguntasCompletadas.Count} preguntas completadas para el usuario {idUsuario}");
            return preguntasCompletadas;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener preguntas completadas: {ex.Message}");
            throw;
        }
    }

    /// Obtiene los IDs de todas las preguntas que un usuario ha completado correctamente
    public async Task<List<int>> ObtenerIdsPreguntasCompletadasPorUsuario(int idUsuario)
    {
        List<int> idsCompletados = new List<int>();

        if (_conn.State != System.Data.ConnectionState.Open)
            await _conn.OpenAsync();

        try
        {
            string query = "SELECT id_pregunta FROM usuario_pregunta WHERE id_usuario = @idUsuario AND completado = 1";

            MySqlCommand command = new MySqlCommand(query, _conn);
            command.Parameters.AddWithValue("@idUsuario", idUsuario);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    idsCompletados.Add(reader.GetInt32("id_pregunta"));
                }
            }

            return idsCompletados;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener IDs de preguntas completadas: {ex.Message}");
            throw;
        }
    }

    /// Reinicia el progreso de un usuario eliminando todas sus preguntas completadas
    public async Task ReiniciarProgresoUsuario(int idUsuario)
    {
        if (_conn.State != System.Data.ConnectionState.Open)
            await _conn.OpenAsync();

        try
        {
            string query = "DELETE FROM usuario_pregunta WHERE id_usuario = @idUsuario";

            MySqlCommand command = new MySqlCommand(query, _conn);
            command.Parameters.AddWithValue("@idUsuario", idUsuario);

            int filasAfectadas = await command.ExecuteNonQueryAsync();
            Debug.Log($"Se reinició el progreso del usuario {idUsuario}. Registros eliminados: {filasAfectadas}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al reiniciar progreso del usuario: {ex.Message}");
            throw;
        }
    }

    /// Versión asíncrona para verificar si un usuario ya completó correctamente una pregunta específica
    public async Task<bool> YaCompletoLaPreguntaAsync(int idUsuario, int idPregunta)
    {
        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();

            // Consulta simplificada: verifica solo la existencia del registro
            var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM usuario_pregunta WHERE id_usuario = @u AND id_pregunta = @p",
                _conn
            );
            cmd.Parameters.AddWithValue("@u", idUsuario);
            cmd.Parameters.AddWithValue("@p", idPregunta);

            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0; // True si existe, False si no
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al verificar completitud: {ex.Message}");
            return false;
        }
    }
}