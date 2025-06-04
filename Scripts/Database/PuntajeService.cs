using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using UnityEditor.Search;
using UnityEngine;
using static UsuarioService;

public class PuntajeService
{
    private readonly MySqlConnection _conn;

    public PuntajeService(MySqlConnection conn)
    {
        _conn = conn;
    }

    public class Puntaje
    {
        public int Id { get; set; }
        public int scoreTotal { get; set; }
        public int scorexespecialidad { get; set; }
        public int id_especialidad { get; set; }
        public int id_usuario { get; set; }
        public DateTime fecha { get; set; }
    }

    public Puntaje Seleccionar(int id)
    {
        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();

            string query = @"SELECT * FROM puntaje WHERE id_puntaje = @id";
            using (var cmd = new MySqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new Puntaje
                    {
                        Id = Convert.ToInt32(reader["id_puntaje"]),
                        scoreTotal = Convert.ToInt32(reader["scoreTotal"]),
                        id_especialidad = Convert.ToInt32(reader["id_especialidad"]),
                        id_usuario = Convert.ToInt32(reader["id_usuario"]),
                        fecha = Convert.ToDateTime(reader["fecha"])
                    };

                }
                return null;  // Retornar null si no se encuentra el puntaje
            }
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al buscar usuario: " + e.Message);
            return null;
        }
    }

    /// Inserta un puntaje por especialidad
    public async Task<bool> Crear(int scorexEspecialidad, int id_usuario, int id_especialidad)
    {
        var cmd = new MySqlCommand("INSERT INTO puntaje (scorexEspecialidad, id_usuario, fecha, id_especialidad)" +
            " VALUES (@scorexEspecialidad, @id_usuario, @fecha, @id_especialidad)", _conn);
        cmd.Parameters.AddWithValue("@scorexEspecialidad", scorexEspecialidad);
        cmd.Parameters.AddWithValue("@id_usuario", id_usuario);
        cmd.Parameters.AddWithValue("@fecha", System.DateTime.Now); // Fecha actual
        cmd.Parameters.AddWithValue("@id_especialidad", id_especialidad);

        return cmd.ExecuteNonQuery() > 0;
    }

    //Actualizar el score total
    public bool Actualizar(int scoreTotal, int id_usuario)
    {
        var cmd = new MySqlCommand("UPDATE puntaje SET scoreTotal = @scoreTotal WHERE id_usuario = @id_usuario", _conn);
        cmd.Parameters.AddWithValue("@scoreTotal", scoreTotal);
        cmd.Parameters.AddWithValue("@id_usuario", id_usuario);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Eliminar(int id)
    {
        var cmd = new MySqlCommand("DELETE FROM puntaje WHERE id_puntaje = @id", _conn);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public int CalcularScoreTotal(int id)
    {
        int scoreTotal = 0;

        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();

            // Query que obtiene el puntaje máximo por cada especialidad para el usuario
            string query = @"SELECT MAX(scorexEspecialidad) as MaxScore 
                        FROM puntaje 
                        WHERE id_usuario = @id_usuario 
                        GROUP BY id_especialidad";

            using (var cmd = new MySqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@id_usuario", id);
                using var reader = cmd.ExecuteReader();

                // Sumar todos los puntajes máximos de cada especialidad
                while (reader.Read())
                {
                    int maxScorexEspecialidad = Convert.ToInt32(reader["MaxScore"]);
                    scoreTotal += maxScorexEspecialidad;
                }
            }
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al calcular score total: " + e.Message);
            return 0;
        }

        return scoreTotal;
    }

    public async Task<PlayerScores> GetPlayerScoresByIdAsync(int idUsuario)
    {
        try
        {
           
                int totalScore = CalcularScoreTotal(idUsuario);

                await Task.Delay(100);

                return new PlayerScores
                {
                    GlobalScore = totalScore
                };
            
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al obtener puntajes del jugador: " + e.Message);
            await Task.Delay(100);
            return new PlayerScores { GlobalScore = 0 };
        }
    }

    public async Task<SpecialtyScores> GetSpecialtyScoresByIdAsync(int idUsuario)
    {
        var specialtyScores = new SpecialtyScores();

        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();

            // Obtener los puntajes agrupados por especialidad para el usuario
            string query = @"SELECT id_especialidad, SUM(scorexEspecialidad) as TotalScore 
                           FROM puntaje 
                           WHERE id_usuario = @id_usuario 
                           GROUP BY id_especialidad";

            using (var cmd = new MySqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int especialidadId = Convert.ToInt32(reader["id_especialidad"]);
                    int score = Convert.ToInt32(reader["TotalScore"]);

                    specialtyScores.ScoresBySpecialty[especialidadId] = score;
                }
            }
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al obtener puntajes por especialidad: " + e.Message);
        }

        await Task.Delay(100);
        return specialtyScores;
    }

    public async Task<bool> UpdateSpecialtyScoreAsync(int idUsuario, int idEspecialidad, int score)
    {
        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();

            // Verificar si ya existe un registro para esta combinación usuario-especialidad
            string checkQuery = @"SELECT COUNT(*) FROM puntaje WHERE id_usuario = @id_usuario AND id_especialidad = @id_especialidad";

            using (var checkCmd = new MySqlCommand(checkQuery, _conn))
            {
                checkCmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                checkCmd.Parameters.AddWithValue("@id_especialidad", idEspecialidad);

                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count > 0)
                {
                    // Actualizar registro existente
                    string updateQuery = @"UPDATE puntaje SET scorexEspecialidad = @score, fecha = @fecha 
                                         WHERE id_usuario = @id_usuario AND id_especialidad = @id_especialidad";

                    using (var updateCmd = new MySqlCommand(updateQuery, _conn))
                    {
                        updateCmd.Parameters.AddWithValue("@score", score);
                        updateCmd.Parameters.AddWithValue("@fecha", DateTime.Now);
                        updateCmd.Parameters.AddWithValue("@id_usuario", idUsuario);
                        updateCmd.Parameters.AddWithValue("@id_especialidad", idEspecialidad);

                        await Task.Delay(100);
                        return updateCmd.ExecuteNonQuery() > 0;
                    }
                }
                else
                {
                    // Crear nuevo registro
                    await Crear(score, idUsuario, idEspecialidad);
                    return true;
                }
            }
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al actualizar puntaje de especialidad: " + e.Message);
            await Task.Delay(100);
            return false;
        }
    }

    public async Task<bool> UpdateGlobalScoreAsync(int idUsuario, int score)
    {
        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();

            // Actualizar el score total para el usuario
            string query = @"UPDATE puntaje SET scoreTotal = @scoreTotal WHERE id_usuario = @id_usuario";

            using (var cmd = new MySqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@scoreTotal", score);
                cmd.Parameters.AddWithValue("@id_usuario", idUsuario);

                await Task.Delay(100);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al actualizar puntaje global: " + e.Message);
            await Task.Delay(100);
            return false;
        }
    }
}