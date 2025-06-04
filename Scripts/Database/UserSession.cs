using UnityEngine;

public class UserSession
{
    private static UserSession _instance;

    public static UserSession Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UserSession();
            }
            return _instance;
        }
    }

    public int IdUsuario { get; private set; }
    public void SetUserId(int id)
    {
        IdUsuario = id;
    }
    public int RolUsuario { get; private set; }

    public void SetRolUsuario(int rol)
    {
        RolUsuario = rol;
    }
    public int EspecialidadActual { get; set; }

    public void SetEspecialidadActual(int especialidad)
    {
        EspecialidadActual = especialidad;
    }

    private UserSession() { } // Constructor privado para asegurar Singleton
}
