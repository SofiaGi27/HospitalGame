using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using MySqlConnector;
using UnityEngine;

// Clase para gestionar códigos de verificación
public class EmailVerificationService
{
    private readonly MySqlConnection _conn;

    // Hacer el diccionario estático para que persista entre instancias
    private static readonly Dictionary<string, VerificationCode> _verificationCodes = new Dictionary<string, VerificationCode>();

    public EmailVerificationService(MySqlConnection conn)
    {
        _conn = conn;
    }

    // Estructura para almacenar códigos de verificación
    private class VerificationCode
    {
        public string Code { get; set; }
        public DateTime ExpirationTime { get; set; }
        public string Email { get; set; }
        public bool IsUsed { get; set; }
    }

    // Generar código aleatorio de 6 dígitos
    private string GenerateVerificationCode()
    {
        System.Random random = new System.Random();
        return random.Next(100000, 999999).ToString();
    }

    // Verificar si el email existe en la base de datos
    public bool EmailExists(string email)
    {
        try
        {
            string query = "SELECT COUNT(*) FROM usuario WHERE email = @email";
            using (var cmd = new MySqlCommand(query, _conn))
            {
                cmd.Parameters.AddWithValue("@email", email);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }
        catch (MySqlException e)
        {
            Debug.LogError("❌ Error al verificar email: " + e.Message);
            return false;
        }
    }

    // Enviar código de verificación por email
    public bool SendVerificationCode(string email)
    {
        try
        {
            // Verificar si el email existe
            if (!EmailExists(email))
            {
                Debug.LogWarning("⚠️ El email no existe en la base de datos.");
                return false;
            }

            // Limpiar código anterior si existe
            if (_verificationCodes.ContainsKey(email))
            {
                _verificationCodes.Remove(email);
                Debug.Log($"🔄 Código anterior removido para {email}");
            }

            // Generar código
            string code = GenerateVerificationCode();
            DateTime expiration = DateTime.Now.AddMinutes(10); // Expira en 10 minutos

            // Almacenar código
            _verificationCodes[email] = new VerificationCode
            {
                Code = code,
                ExpirationTime = expiration,
                Email = email,
                IsUsed = false
            };

            Debug.Log($"📝 Código generado para {email}: {code} (Expira: {expiration})");
            Debug.Log($"📊 Total de códigos almacenados: {_verificationCodes.Count}");

            // Enviar email
            bool emailSent = SendEmail(email, code);

            if (emailSent)
            {
                Debug.Log($"✅ Código de verificación enviado a {email}");
                return true;
            }
            else
            {
                // Remover código si no se pudo enviar el email
                _verificationCodes.Remove(email);
                Debug.LogError($"❌ No se pudo enviar email, código removido para {email}");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error al enviar código de verificación: " + e.Message);
            return false;
        }
    }

    // Método para enviar email 
    private bool SendEmail(string toEmail, string verificationCode)
    {
        try
        {
            // CONFIGURACIÓN DEL SERVIDOR SMTP -
            string smtpServer = "smtp.gmail.com"; 
            int smtpPort = 587;
            string senderEmail = "hospitaljuego@gmail.com"; 
            string senderPassword = "otyh ztbx emjg mwao"; // Contraseña de aplicación

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(senderEmail, "Huparchis");
            mail.To.Add(toEmail);
            mail.Subject = "Código de Verificación para Huparchis - Cambio de Contraseña";
            mail.Body = $@"
                <html>
                <body>
                    <h2>Código de Verificación</h2>
                    <p>Has solicitado cambiar tu contraseña.</p>
                    <p><strong>Tu código de verificación es: {verificationCode}</strong></p>
                    <p>Este código expira en 10 minutos.</p>
                    <p>Si no solicitaste este cambio, ignora este correo.</p>
                </body>
                </html>";
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
            smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
            smtp.EnableSsl = true;

            smtp.Send(mail);
            Debug.Log($"📧 Email enviado exitosamente a {toEmail}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error al enviar email: " + e.Message);
            return false;
        }
    }

    // Verificar código ingresado por el usuario
    public bool VerifyCode(string email, string inputCode)
    {
        try
        {
            Debug.Log($"🔍 Verificando código para email: {email}");
            Debug.Log($"🔍 Código ingresado: {inputCode}");
            Debug.Log($"📊 Códigos almacenados: {_verificationCodes.Count}");

            // Mostrar todos los emails que tienen códigos almacenados
            foreach (var kvp in _verificationCodes)
            {
                Debug.Log($"📋 Email en diccionario: {kvp.Key} - Código: {kvp.Value.Code} - Usado: {kvp.Value.IsUsed} - Expira: {kvp.Value.ExpirationTime}");
            }

            if (!_verificationCodes.ContainsKey(email))
            {
                Debug.LogWarning($"⚠️ No hay código de verificación para este email: {email}");
                Debug.LogWarning("📝 Emails disponibles en el diccionario:");
                foreach (var key in _verificationCodes.Keys)
                {
                    Debug.LogWarning($"   - {key}");
                }
                return false;
            }

            var verificationData = _verificationCodes[email];

            // Verificar si ya fue usado
            if (verificationData.IsUsed)
            {
                Debug.LogWarning("⚠️ Este código ya fue utilizado.");
                return false;
            }

            // Verificar si expiró
            if (DateTime.Now > verificationData.ExpirationTime)
            {
                Debug.LogWarning($"⚠️ El código de verificación ha expirado. Hora actual: {DateTime.Now}, Expiración: {verificationData.ExpirationTime}");
                _verificationCodes.Remove(email);
                return false;
            }

            // Verificar código
            if (verificationData.Code == inputCode)
            {
                verificationData.IsUsed = true; // Marcar como usado
                Debug.Log("✅ Código de verificación correcto.");
                return true;
            }
            else
            {
                Debug.LogWarning($"⚠️ Código de verificación incorrecto. Esperado: {verificationData.Code}, Recibido: {inputCode}");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error al verificar código: " + e.Message);
            return false;
        }
    }

    // Limpiar código después de usar
    public void ClearVerificationCode(string email)
    {
        if (_verificationCodes.ContainsKey(email))
        {
            _verificationCodes.Remove(email);
            Debug.Log($"🧹 Código limpiado para {email}");
        }
    }

    // Limpiar códigos expirados (llamar periódicamente)
    public void CleanExpiredCodes()
    {
        var expiredKeys = new List<string>();

        foreach (var kvp in _verificationCodes)
        {
            if (DateTime.Now > kvp.Value.ExpirationTime)
            {
                expiredKeys.Add(kvp.Key);
            }
        }

        foreach (var key in expiredKeys)
        {
            _verificationCodes.Remove(key);
            Debug.Log($"🧹 Código expirado removido para {key}");
        }

        if (expiredKeys.Count > 0)
        {
            Debug.Log($"🧹 Se limpiaron {expiredKeys.Count} códigos expirados");
        }
    }

    // Método para debuggear - mostrar todos los códigos almacenados
    //public void DebugShowAllCodes()
    //{
    //    Debug.Log($"🐛 DEBUG - Total de códigos: {_verificationCodes.Count}");
    //    foreach (var kvp in _verificationCodes)
    //    {
    //        Debug.Log($"🐛 DEBUG - Email: {kvp.Key}, Código: {kvp.Value.Code}, Usado: {kvp.Value.IsUsed}, Expira: {kvp.Value.ExpirationTime}");
    //    }
    //}
}