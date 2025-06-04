using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using UnityEngine;
using static UsuarioService;

public class EspecialidadService
{
    private readonly MySqlConnection _conn;

    public EspecialidadService(MySqlConnection conn)
    {
        _conn = conn;
    }

    public class Especialidad
    {
        public int Id { get; set; }
        public string Name { get; set; }    
        public int id_rol {  get; set; }
    }

    /// Inserta una especialidad
    public bool Crear(string nombre,int id_rol)
    {
        var cmd = new MySqlCommand("INSERT INTO especialidad (nombre) VALUES (@nombre, @id_rol)", _conn);
        cmd.Parameters.AddWithValue("@nombre", nombre);
        cmd.Parameters.AddWithValue("@id_rol", id_rol);
        return cmd.ExecuteNonQuery() > 0;
    }

    /// Lee las especialidades y las devuelve en una lista
    public List<Especialidad> LeerTodas()
    {
        List<Especialidad> especialidades = new List<Especialidad>();
        var cmd = new MySqlCommand("SELECT * FROM especialidad", _conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Especialidad especialidad = new Especialidad
            {
                Id = Convert.ToInt32(reader["id_especialidad"]),
                Name = reader["nombre"].ToString(),

            };

            especialidades.Add(especialidad);
            ///Debug.Log($"ID: {especialidad.Id}, Name: {especialidad.nombre}");
        }
        return especialidades;
    }
    //Lee las especialidades según el rol
    public List<Especialidad> LeerTodasPorRol(int id_rol)
    {
        List<Especialidad> especialidades = new List<Especialidad>();
        var cmd = new MySqlCommand("SELECT * FROM especialidad where id_rol=@id_rol", _conn);
        cmd.Parameters.AddWithValue("id_rol", id_rol);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Especialidad especialidad = new Especialidad
            {
                Id = Convert.ToInt32(reader["id_especialidad"]),
                Name = reader["nombre"].ToString(),

            };

            especialidades.Add(especialidad);
            ///Debug.Log($"ID: {especialidad.Id}, Name: {especialidad.nombre}");
        }
        return especialidades;
    }
    public String GetEspecialidadName(int id_especialidad)
    {
        String Name="";
        var cmd = new MySqlCommand("SELECT nombre FROM especialidad where id_especialidad=@id_especialidad", _conn);
        cmd.Parameters.AddWithValue("id_especialidad", id_especialidad);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            Name = reader["nombre"].ToString();
        };
        return Name;

    }

    /// Cambia el nombre de la especialidad
    public bool Actualizar(int id, string nuevoNombre)
    {
        var cmd = new MySqlCommand("UPDATE especialidad SET nombre = @nombre WHERE id_especialidad = @id", _conn);
        cmd.Parameters.AddWithValue("@nombre", nuevoNombre);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    /// Elimina una especialidad por su id
    public bool Eliminar(int id)
    {
        var cmd = new MySqlCommand("DELETE FROM especialidad WHERE id_especialidad = @id", _conn);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }
    //Devuelve una lista de especialidades por rol de manera asincrona
    public async Task<List<Especialidad>> GetEspecialidadesAsync(int id_rol)
    {
       
        List<Especialidad> especialidades = LeerTodasPorRol(id_rol);
        await Task.Delay(100);

        return especialidades;
       
    }
}