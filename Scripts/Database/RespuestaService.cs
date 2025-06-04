using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class Respuesta
{
    public int Id { get; set; }
    public int PreguntaId { get; set; }
    public string TextoRespuesta { get; set; }
    public bool EsCorrecta { get; set; }

    public override string ToString()
    {
        return $"Respuesta[ID={Id}, PreguntaID={PreguntaId}, Texto={TextoRespuesta}, Correcta={EsCorrecta}]";
    }
}

public class RespuestaService
{
    private readonly MySqlConnection _conn;

    public RespuestaService(MySqlConnection conn)
    {
        _conn = conn;
    }
    public RespuestaService GetRespuestaService()
    {
        return this;
    }
    // Método para crear una nueva respuesta
    public bool Crear(int idPregunta, string respuesta, bool esCorrecta)
    {
        if (string.IsNullOrEmpty(respuesta)) return false;

        var cmd = new MySqlCommand(
            "INSERT INTO respuesta (id_pregunta, texto, es_correcta) VALUES (@idPregunta, @respuesta, @esCorrecta)",
            _conn);

        cmd.Parameters.AddWithValue("@idPregunta", idPregunta);
        cmd.Parameters.AddWithValue("@respuesta", respuesta);
        cmd.Parameters.AddWithValue("@esCorrecta", esCorrecta ? 1 : 0);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Método para actualizar una respuesta existente
    public bool Actualizar(int id, string nuevaRespuesta, bool esCorrecta)
    {
        if (string.IsNullOrEmpty(nuevaRespuesta)) return false;

        var cmd = new MySqlCommand(
            "UPDATE respuesta SET texto = @respuesta, es_correcta = @esCorrecta WHERE id_respuesta = @id",
            _conn);

        cmd.Parameters.AddWithValue("@respuesta", nuevaRespuesta);
        cmd.Parameters.AddWithValue("@esCorrecta", esCorrecta ? 1 : 0);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }

    // Método para eliminar una respuesta
    public bool Eliminar(int id)
    {
        var cmd = new MySqlCommand("DELETE FROM respuesta WHERE id_respuesta = @id", _conn);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    // Método para obtener todas las respuestas para una pregunta específica
    public async Task<List<Respuesta>> ObtenerRespuestasPorPreguntaId(int preguntaId)
    {
        var respuestas = new List<Respuesta>();

        try
        {
            // Asegurarse de que la conexión está abierta
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();

            using (var cmd = new MySqlCommand(
                "SELECT id_respuesta, id_pregunta, texto, es_correcta FROM respuesta WHERE id_pregunta = @preguntaId",
                _conn))
            {
                cmd.Parameters.AddWithValue("@preguntaId", preguntaId);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        respuestas.Add(new Respuesta
                        {
                            Id = reader.GetInt32(0),
                            PreguntaId = reader.GetInt32(1),
                            TextoRespuesta = reader.GetString(2),
                            EsCorrecta = reader.GetBoolean(3)
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener respuestas para la pregunta {preguntaId}: {ex.Message}");
        }

        return respuestas;
    }

    // Método para obtener una respuesta específica por ID
    public async Task<Respuesta> ObtenerRespuestaPorId(int id)
    {
        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();

            using (var cmd = new MySqlCommand(
                "SELECT id_respuesta, id_pregunta, texto, es_correcta FROM respuesta WHERE id_respuesta = @id",
                _conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Respuesta
                        {
                            Id = reader.GetInt32(0),
                            PreguntaId = reader.GetInt32(1),
                            TextoRespuesta = reader.GetString(2),
                            EsCorrecta = reader.GetBoolean(3)
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener respuesta por ID: {ex.Message}");
        }

        return null;
    }

    // Método para verificar si una respuesta es correcta
    public async Task<bool> VerificarRespuesta(int idRespuesta)
    {
        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();

            using (var cmd = new MySqlCommand("SELECT es_correcta FROM respuesta WHERE id_respuesta = @id", _conn))
            {
                cmd.Parameters.AddWithValue("@id", idRespuesta);

                object result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToBoolean(result);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al verificar respuesta: {ex.Message}");
        }

        return false;
    }
}