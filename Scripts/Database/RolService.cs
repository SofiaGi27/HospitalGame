using System;
using System.Collections.Generic;
using MySqlConnector;
using UnityEngine;
using static UsuarioService;
public class RolService
{
    private readonly MySqlConnection _conn;

    public class Rol
    {
        public int id { get; set; }
        public string _name { get; set; }
    }
    public RolService(MySqlConnection conn)
    {
        _conn = conn;
    }

    /// <summary>
    /// Lee todos los roles y los devuelve en una lista
    /// </summary>
    public List<Rol> LeerTodos()
    {
        List<Rol> roles = new List<Rol>();
        var cmd = new MySqlCommand("SELECT * FROM rol", _conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Rol rol = new Rol
            {
                id = Convert.ToInt32(reader["id_rol"]),
                _name = reader["nombre"].ToString(),
            };

            roles.Add(rol);
        }
        return roles;
    }
    public bool Crear(string _name)
    {
        if (string.IsNullOrEmpty(_name)) return false;
        var cmd = new MySqlCommand("INSERT INTO rol (_name) VALUES (@name)", _conn);
        cmd.Parameters.AddWithValue("@name", _name);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Actualizar(int id, string nuevoNombre)
    {
        if (string.IsNullOrEmpty(nuevoNombre)) return false;
        var cmd = new MySqlCommand("UPDATE rol SET nombre = @name WHERE id_rol = @id", _conn);
        cmd.Parameters.AddWithValue("@name", nuevoNombre);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Eliminar(int id)
    {
        var cmd = new MySqlCommand("DELETE FROM rol WHERE id_rol = @id", _conn);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }
}