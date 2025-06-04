using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System;

public class Pregunta
{
    public int Id { get; set; }
    public int IdEspecialidad { get; set; }
    public string TextoPregunta { get; set; }

    public override string ToString()
    {
        return $"Pregunta[ID={Id}, IdEspecialidad={IdEspecialidad}, Texto={TextoPregunta}]";
    }
}

public class PreguntaService
{
    private readonly MySqlConnection _conn;

    public PreguntaService(MySqlConnection conn)
    {
        _conn = conn;
    }
    public PreguntaService GetPreguntaService()
    {
        return this;
    }

    public bool Crear(int idEspecialidad, string pregunta, string rol)
    {
        if (string.IsNullOrEmpty(pregunta)) return false;
        var cmd = new MySqlCommand("INSERT INTO pregunta (id_especialidad, pregunta, rol) VALUES (@idEspecialidad, @pregunta, @rol)", _conn);
        cmd.Parameters.AddWithValue("@idEspecialidad", idEspecialidad);
        cmd.Parameters.AddWithValue("@pregunta", pregunta);
        cmd.Parameters.AddWithValue("@rol", rol);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Actualizar(int id, string nuevaPregunta)
    {
        if (string.IsNullOrEmpty(nuevaPregunta)) return false;
        var cmd = new MySqlCommand("UPDATE pregunta SET pregunta = @pregunta WHERE id_pregunta = @id", _conn);
        cmd.Parameters.AddWithValue("@pregunta", nuevaPregunta);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Eliminar(int id)
    {
        var cmd = new MySqlCommand("DELETE FROM pregunta WHERE id_pregunta = @id", _conn);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public int ObtenerUltimoIdInsertado()
    {
        using (var cmd = new MySqlCommand("SELECT LAST_INSERT_ID()", _conn))
        {
            object result = cmd.ExecuteScalar();
            if (result != null && result != System.DBNull.Value)
            {
                return System.Convert.ToInt32(result);
            }
            else
            {
                return -1;
            }
        }
    }

    /// Obtiene una lista con todas las preguntas
    public async Task<List<Pregunta>> ObtenerTodasLasPreguntas()
    {
        var preguntas = new List<Pregunta>();

        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();

            using (var cmd = new MySqlCommand("SELECT id_pregunta, id_especialidad, pregunta FROM pregunta", _conn))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        preguntas.Add(new Pregunta
                        {
                            Id = reader.GetInt32(0),
                            IdEspecialidad = reader.GetInt32(1),
                            TextoPregunta = reader.GetString(2)
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener preguntas: {ex.Message}");
        }

        return preguntas;
    }

    // Método para obtener preguntas por especialidad
    public async Task<List<Pregunta>> ObtenerPreguntasPorEspecialidad(int idEspecialidad)
    {
        var preguntas = new List<Pregunta>();

        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();

            using (var cmd = new MySqlCommand("SELECT id_pregunta, id_especialidad, pregunta FROM pregunta WHERE id_especialidad = @idEspecialidad " , _conn))
            {
                cmd.Parameters.AddWithValue("@idEspecialidad", idEspecialidad);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        preguntas.Add(new Pregunta
                        {
                            Id = reader.GetInt32(0),
                            IdEspecialidad = reader.GetInt32(1),
                            TextoPregunta = reader.GetString(2)
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener preguntas por especialidad: {ex.Message}");
        }

        return preguntas;
    }

    // Método para obtener una pregunta específica por ID
    public async Task<Pregunta> ObtenerPreguntaPorId(int id)
    {
        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();

            using (var cmd = new MySqlCommand("SELECT id_pregunta, id_especialidad, pregunta FROM pregunta WHERE id_pregunta = @id", _conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Pregunta
                        {
                            Id = reader.GetInt32(0),
                            IdEspecialidad = reader.GetInt32(1),
                            TextoPregunta = reader.GetString(2)
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al obtener pregunta por ID: {ex.Message}");
        }

        return null;
    }

    // Método para contar el número total de preguntas
    public async Task<int> ContarPreguntas()
    {
        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                await _conn.OpenAsync();

            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM pregunta", _conn))
            {
                object result = await cmd.ExecuteScalarAsync();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt32(result);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error al contar preguntas: {ex.Message}");
        }

        return 0;
    }
}