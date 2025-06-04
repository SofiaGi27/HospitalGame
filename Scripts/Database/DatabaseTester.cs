using UnityEngine;
using MySqlConnector; // Asegúrate de usar este namespace

public class DatabaseTester : MonoBehaviour
{
    private MySQLManager dbManager;

    void Start()
    {
        dbManager = FindAnyObjectByType<MySQLManager>();

        // 1. Registrar un usuario nuevo
        //dbManager.especialidadService.Crear("Cardiología");
        //Debug.Log("Especialidad registrada.");

        // 2. Verificar login (debería retornar true)
        //bool loginExitoso = dbManager.usuarioService.CheckLogin("test@example.com", "password123");
        //Debug.Log(loginExitoso ? "✅ Login correcto" : "❌ Login fallido");

        // 3. Verificar login con contraseña incorrecta (debería retornar false)
        //bool loginFallido = dbManager.usuarioService.CheckLogin("test@example.com", "contraseñaErronea");
        //Debug.Log(loginFallido ? "✅ Login (inesperado)" : "❌ Login fallido (esperado)");
    }
}