using MySqlConnector;
using UnityEngine;
public class PersonajeService
{
    private readonly MySqlConnection _conn;

    public PersonajeService(MySqlConnection conn)
    {
        _conn = conn;
    }

    public bool Crear(string descripcion)
    {
        if (string.IsNullOrEmpty(descripcion)) return false;
        var cmd = new MySqlCommand("INSERT INTO personaje (descripcion) VALUES (@descripcion)", _conn);
        cmd.Parameters.AddWithValue("@descripcion", descripcion);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Actualizar(int id, string nuevaDescripcion)
    {
        if (string.IsNullOrEmpty(nuevaDescripcion)) return false;
        var cmd = new MySqlCommand("UPDATE personaje SET descripcion = @descripcion WHERE id_skin = @id", _conn);
        cmd.Parameters.AddWithValue("@descripcion", nuevaDescripcion);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }

    public bool Eliminar(int id)
    {
        var cmd = new MySqlCommand("DELETE FROM personaje WHERE id_skin = @id", _conn);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;
    }
}