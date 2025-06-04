using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlConnector;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;


public class UsuarioService
{
    private readonly MySqlConnection _conn;

    public EmailVerificationService _emailVerification { get; private set; }

    public UsuarioService(MySqlConnection conn)
    {
        _conn = conn;
        _emailVerification = new EmailVerificationService(_conn);
    }


    public class Usuario
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public int Monedas { get; set; }
        public int IdSkin { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RolUsuario { get; set; }
    }

    public Usuario Seleccionar(int id)
    {
        try
        {
            if (_conn.State != System.Data.ConnectionState.Open)
                _conn.Open();

            string query = @"SELECT * FROM usuario WHERE id_usuario = @id";
            using (var cmd = new MySqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())  
                {
                    Usuario usuario = new Usuario
                    {
                        Id = Convert.ToInt32(reader["id_usuario"]),
                        Email = reader["email"].ToString(),
                        Username = reader["user"].ToString(),
                        Monedas = Convert.ToInt32(reader["monedas"]),
                        IdSkin = Convert.ToInt32(reader["id_skin"]),
                        CreatedAt = Convert.ToDateTime(reader["created_at"]),
                        RolUsuario = Convert.ToInt32(reader["rol_usuario"])

                    };
                    return usuario;
                }
                return null;  // Retornar null si no se encuentra el usuario
            }
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al buscar usuario: " + e.Message);
            return null;
        }
        
    }
    /// Actualiza el nombre de usuario y las monedas
    public bool Actualizar(int id, string nuevoUser, int nuevasMonedas)
    {
        var cmd = new MySqlCommand("UPDATE usuario SET user = @user, monedas = @monedas WHERE id_usuario = @id", _conn);
        cmd.Parameters.AddWithValue("@user", nuevoUser);
        cmd.Parameters.AddWithValue("@monedas", nuevasMonedas);
        cmd.Parameters.AddWithValue("@id", id);
        return cmd.ExecuteNonQuery() > 0;

    }

    /// Elimina a un usuario por su número de usuario
    private void Eliminar(int id)
    {
        var cmd = new MySqlCommand("DELETE FROM usuario WHERE id_usuario = @id", _conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }



    // Registrar usuario con BCrypt
    /// Se inserta el usuario con la contraseña previamente hasheada
    /// monedas por defecto 0
    public bool Crear(string email, string plainPassword, string username, int role)
    {
        try
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword, BCrypt.Net.BCrypt.GenerateSalt(12));
            string query = @"INSERT INTO usuario (email, pass, user, monedas, id_skin, created_at, rol_usuario)
                         VALUES (@email, @pass, @user, @monedas, @id_skin, @created_at, @rol_usuario)";

            using (var cmd = new MySqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@pass", hashedPassword);
                cmd.Parameters.AddWithValue("@user", username);
                cmd.Parameters.AddWithValue("@monedas", 0); // Valor inicial
                cmd.Parameters.AddWithValue("@id_skin", 1); // ID skin por defecto
                cmd.Parameters.AddWithValue("@created_at", System.DateTime.Now); // Fecha actual
                cmd.Parameters.AddWithValue("@rol_usuario", role); // Valor fk int id del rol
                cmd.ExecuteNonQuery();
            }
            return true;
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al registrar usuario: " + e.Message);
            return false;
        }
    }


    // Verificar login
    public bool CheckLogin(string email, string plainPassword)
    {
        try
        {
            string query = "SELECT id_usuario,rol_usuario, pass FROM usuario WHERE email = @email";
            using (var cmd = new MySqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@email", email);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string storedHash = reader.GetString("pass");
                        ///Verifica que al codificar la contraseña sin hashear con el codigo hash guardado sea igual a la contraseña en la base de datos
                        if (BCrypt.Net.BCrypt.Verify(plainPassword, storedHash))
                        {
                            ///Guarda el id y rol del usuario en una clase estatica 
                            int idUsuario = reader.GetInt32("id_usuario");
                            int rolUsuario = reader.GetInt32("rol_usuario");
                            UserSession.Instance.SetUserId(idUsuario); // Guarda en el singleton
                            UserSession.Instance.SetRolUsuario(rolUsuario);
                            Debug.Log($"✅ Login correcto. ID de usuario: {idUsuario}");
                            return true;
                        }
                    }
                }
            }
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al verificar login: " + e.Message);
        }
        return false;
    }

    /// <summary>
    /// Mètodo privado 
    /// Cambia la contraseña hasheandola previamente 
    /// </summary>
    private bool ChangePassword(string email, string newPassword)
    {
        try
        {
            string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, BCrypt.Net.BCrypt.GenerateSalt(12));
            string query = "UPDATE usuario SET pass = @newPass WHERE email = @email";

            using (var cmd = new MySqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@newPass", newHashedPassword);
                cmd.Parameters.AddWithValue("@email", email);

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    Debug.Log("🔑 Contraseña actualizada con éxito.");
                    return true;
                }
                else
                {
                    Debug.LogWarning("⚠️ No se encontró el usuario para actualizar la contraseña.");
                    return false;
                }
            }
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al cambiar la contraseña: " + e.Message);
            return false;
        }
    }


    /// <summary>
    /// Verifica que el usuario tenga acceso al correo electrónico 
    /// enviandole un código
    /// </summary>
    public bool ChangePasswordWithVerification(string email, string verificationCode, string newPassword)
    {
        try
        {
            // Verificar código
            if (!_emailVerification.VerifyCode(email, verificationCode))
            {
                return false;
            }

            // Si el código es correcto, cambiar contraseña
            bool passwordChanged = ChangePassword(email, newPassword);

            if (passwordChanged)
            {
                // Limpiar código de verificación
                _emailVerification.ClearVerificationCode(email);
                Debug.Log("🔑 Contraseña cambiada exitosamente con verificación.");
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error en el proceso de cambio de contraseña: " + e.Message);
            return false;
        }
    }

    // Limpiar códigos expirados periódicamente
    public void CleanExpiredVerificationCodes()
    {
        _emailVerification.CleanExpiredCodes();
    }

    /// <summary>
    /// Lee todos los usuarios y los devuelve en una lista
    /// </summary>
    public List<Usuario> LeerTodos()
    {
        List<Usuario> usuarios = new List<Usuario>();
        var cmd = new MySqlCommand("SELECT * FROM usuario", _conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            Usuario usuario = new Usuario
            {
                Id = Convert.ToInt32(reader["id_usuario"]),
                Email = reader["email"].ToString(),
                Username = reader["user"].ToString(),
                Monedas = Convert.ToInt32(reader["monedas"]),
                IdSkin = Convert.ToInt32(reader["id_skin"]),
                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                RolUsuario = Convert.ToInt32(reader["rol_usuario"]),
            };

            usuarios.Add(usuario);
            Debug.Log($"ID: {usuario.Id}, User: {usuario.Username}, Monedas: {usuario.Monedas}, Skin: {usuario.IdSkin}");
        }
        return usuarios;
    }


    // Actualizar nombre de usuario
    public bool UpdateUsername(int idUsuario, string newUsername)
    {
        try
        {
            string query = "UPDATE usuario SET user = @username WHERE id_usuario = @id";
            using (var cmd = new MySqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@username", newUsername);
                cmd.Parameters.AddWithValue("@id", idUsuario);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al actualizar username: " + e.Message);
            return false;
        }
    }
    // Actualizar avatarPath
    public bool UpdateAvatarPath(int idUsuario, string newPath)
    {
        try
        {
            string query = "UPDATE usuario SET avatarPath = @avatar WHERE id_usuario = @id";
            using (var cmd = new MySqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@avatar", newPath);
                cmd.Parameters.AddWithValue("@id", idUsuario);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al actualizar avatar Path: " + e.Message);
            return false;
        }
    }

    // Agregar monedas al usuario
    public bool AddCoins(int idUsuario, int coinsToAdd)
    {
        try
        {
            string query = "UPDATE usuario SET monedas = monedas + @coins WHERE id_usuario = @id";
            using (var cmd = new MySqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@coins", coinsToAdd);
                cmd.Parameters.AddWithValue("@id", idUsuario);

                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al agregar monedas: " + e.Message);
            return false;
        }
    }

    public async Task<UserData> GetUserByIdAsync(int idUsuario)
    {
        await Task.Delay(100); // Simular llamada async

        Usuario usuario=Seleccionar(idUsuario);
        return new UserData
        {
            Name = usuario.Username,
            PlayerId = usuario.Id,
            Money = usuario.Monedas,
            AvatarPath = "Sprites/cat-doctor"
        };
    }

    //Métodos asincronas
    public async Task<bool> UpdateUserNameAsync(int idUsuario, string newName)
    {
        UpdateUsername(idUsuario, newName); 
        await Task.Delay(100);
        return true;
    }

    public async Task<bool> UpdateUserAvatarAsync(int idUsuario, string avatarPath)
    {
        UpdateAvatarPath(idUsuario, avatarPath);
        await Task.Delay(100);
        return true;
    }

    public async Task<bool> UpdateUserMoneyAsync(int idUsuario, int newAmount)
    {
        AddCoins(idUsuario, newAmount);
        await Task.Delay(100);
        return true;
    }
}