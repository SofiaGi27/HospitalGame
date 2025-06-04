using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MySqlConnector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static RolService;

public class AuthUIManager : MonoBehaviour
{
    // Referencia al documento UI
    [SerializeField] private UIDocument uiDocument;

    // Referencias a los elementos de UI existentes
    private TextField nameInput;
    private TextField emailInput;
    private TextField passwordInput;
    private Button loginButton;
    private Button registerButton;
    private Button changePassButton;
    private VisualElement nameInputContainer;
    private VisualElement roleContainer;
    private DropdownField roleDropdown;

    // Nuevos elementos para verificación por email
    private VisualElement verificationContainer;
    private TextField verificationCodeInput;
    private TextField newPasswordInput;
    private Button sendCodeButton;
    private Button verifyAndChangeButton;
    private Button backToLoginButton;

    // Estado y servicios
    private bool isRegistering = false;
    private bool isChangingPassword = false;
    private MySQLManager dbManager;
    private EmailVerificationService emailVerificationService;
    private readonly MySqlConnection _conn;

    // Lista para almacenar los roles específicos de la BD
    private List<Rol> rolesFromDatabase = new List<Rol>();

    // IDs específicos de roles que queremos cargar
    private readonly int[] REQUIRED_ROLE_IDS = { 1, 2, 3 }; // Médico, Enfermero, Farmacéutico

    // Label para mostrar mensajes
    private Label messageLabel;

    // 🎵 Sonido
    [Header("Audio Clips")]
    [SerializeField] private AudioClip ambientClip;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private AudioSource audioSource;

    void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        // Obtener referencias a los elementos de UI existentes
        nameInputContainer = root.Q<VisualElement>("name-container");
        roleContainer = root.Q<VisualElement>("role-container");
        nameInput = root.Q<TextField>("name-input");
        emailInput = root.Q<TextField>("email-input");
        passwordInput = root.Q<TextField>("password-input");
        roleDropdown = root.Q<DropdownField>("role-dropdown");
        loginButton = root.Q<Button>("login-button");
        registerButton = root.Q<Button>("register-button");
        changePassButton = root.Q<Button>("change-password-button");
        messageLabel = root.Q<Label>("message-label");

        // Obtener referencias a los nuevos elementos de verificación
        verificationContainer = root.Q<VisualElement>("verification-container");
        verificationCodeInput = root.Q<TextField>("verification-code-input");
        newPasswordInput = root.Q<TextField>("new-password-input");
        sendCodeButton = root.Q<Button>("send-code-button");
        verifyAndChangeButton = root.Q<Button>("verify-and-change-button");
        backToLoginButton = root.Q<Button>("back-to-login-button");

        // Configurar campos de contraseña
        passwordInput.isPasswordField = true;
        if (newPasswordInput != null)
            newPasswordInput.isPasswordField = true;

        // Ocultar elementos al inicio
        nameInputContainer.style.display = DisplayStyle.None;
        roleContainer.style.display = DisplayStyle.None;
        verificationContainer.style.display = DisplayStyle.None;

        // Registrar eventos de botones existentes
        loginButton.clicked += () => { PlayClick(); Login(); };
        registerButton.clicked += () => { PlayClick(); HandleRegisterButton(); };
        changePassButton.clicked += () => { PlayClick(); ShowPasswordChangeScreen(); };

        // Registrar eventos de nuevos botones
        sendCodeButton.clicked += () => { PlayClick(); SendVerificationCode(); };
        verifyAndChangeButton.clicked += () => { PlayClick(); VerifyCodeAndChangePassword(); };
        backToLoginButton.clicked += () => { PlayClick(); BackToLogin(); };

        // Hover effects
        loginButton.RegisterCallback<MouseEnterEvent>(_ => PlayHover());
        registerButton.RegisterCallback<MouseEnterEvent>(_ => PlayHover());
        changePassButton.RegisterCallback<MouseEnterEvent>(_ => PlayHover());
        sendCodeButton.RegisterCallback<MouseEnterEvent>(_ => PlayHover());
        verifyAndChangeButton.RegisterCallback<MouseEnterEvent>(_ => PlayHover());
        backToLoginButton.RegisterCallback<MouseEnterEvent>(_ => PlayHover());

        // Limpiar mensajes al inicio
        ClearMessage();
    }

    private IEnumerator Start()
    {
        dbManager = FindAnyObjectByType<MySQLManager>();

        if (dbManager == null)
        {
            Debug.LogError("No se encontró el MySQLManager. Asegúrate de que existe en la escena.");
            ShowMessage("Error al conectar con la base de datos", true);
            yield break;
        }

        // Espera hasta que GetConnection() no sea null
        while (dbManager.GetConnection() == null)
        {
            Debug.Log("⏳ Esperando conexión MySQL...");
            yield return null;
        }

        var conn = dbManager.GetConnection();

        if (conn == null)
        {
            Debug.LogError("❌ Conexión MySQL es null.");
            yield break;
        }

        emailVerificationService = new EmailVerificationService(conn);

        // Cargar roles específicos desde la base de datos
        yield return StartCoroutine(LoadSpecificRolesFromDatabase());

        // Inicializar audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = ambientClip;
        audioSource.Play();
    }

    // Método modificado para cargar solo los roles específicos (ID 1, 2, 3)
    private IEnumerator LoadSpecificRolesFromDatabase()
    {
        try
        {
            // Limpiar la lista de roles
            rolesFromDatabase.Clear();

            // Obtener todos los roles desde la base de datos
            List<Rol> allRoles = dbManager.rolService.LeerTodos();

            if (allRoles != null && allRoles.Count > 0)
            {
                // Filtrar solo los roles que necesitamos (ID 1, 2, 3)
                foreach (var rol in allRoles)
                {
                    if (Array.Exists(REQUIRED_ROLE_IDS, id => id == rol.id))
                    {
                        rolesFromDatabase.Add(rol);
                    }
                }

                // Ordenar por ID para mantener consistencia
                rolesFromDatabase.Sort((r1, r2) => r1.id.CompareTo(r2.id));

                if (rolesFromDatabase.Count > 0)
                {
                    // Crear lista de nombres para el dropdown
                    List<string> roleNames = new List<string>();

                    foreach (var rol in rolesFromDatabase)
                    {
                        roleNames.Add(rol._name);
                    }

                    // Configurar el dropdown con los nombres de roles
                    if (roleDropdown != null)
                    {
                        roleDropdown.choices = roleNames;
                        roleDropdown.index = 0; // Seleccionar el primer rol por defecto
                        Debug.Log($"✅ Cargados {rolesFromDatabase.Count} roles específicos desde la base de datos (IDs: 1, 2, 3)");

                        // Mostrar información de debug sobre los roles cargados
                        foreach (var rol in rolesFromDatabase)
                        {
                            Debug.Log($"   - ID: {rol.id}, Nombre: {rol._name}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ No se encontraron los roles específicos (ID 1, 2, 3) en la base de datos");
                    SetDefaultSpecificRoles();
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No se encontraron roles en la base de datos");
                SetDefaultSpecificRoles();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Error al cargar roles específicos desde la base de datos: {e.Message}");
            // Fallback: usar roles por defecto si hay error
            SetDefaultSpecificRoles();
        }

        yield return null;
    }

    // Método fallback para roles específicos por defecto
    private void SetDefaultSpecificRoles()
    {
        rolesFromDatabase.Clear();

        // Crear solo los roles específicos por defecto usando la clase Rol
        rolesFromDatabase.Add(new Rol { id = 1, _name = "Médico" });
        rolesFromDatabase.Add(new Rol { id = 2, _name = "Enfermero" });
        rolesFromDatabase.Add(new Rol { id = 3, _name = "Farmacéutico" });

        List<string> roleNames = new List<string> { "Médico", "Enfermero", "Farmacéutico" };

        if (roleDropdown != null)
        {
            roleDropdown.choices = roleNames;
            roleDropdown.index = 0;
        }

        //Debug.Log("⚠️ Usando roles específicos por defecto (ID 1, 2, 3) debido a error en la carga");
    }

    // Método para obtener el ID del rol seleccionado (sin cambios)
    private int GetSelectedRoleId()
    {
        if (roleDropdown != null && roleDropdown.index >= 0 && roleDropdown.index < rolesFromDatabase.Count)
        {
            return rolesFromDatabase[roleDropdown.index].id;
        }

        Debug.LogWarning("⚠️ No se pudo obtener el ID del rol seleccionado, usando rol por defecto (1)");
        return 1; // Rol por defecto (Médico)
    }

    void Update()
    {
        // Limpiar códigos expirados cada 30 segundos
        if (Time.time % 30f < Time.deltaTime && emailVerificationService != null)
        {
            emailVerificationService.CleanExpiredCodes();
        }
    }

    #region Validación de Contraseña

    /// <summary>
    /// Valida que la contraseña cumpla con los requisitos de seguridad:
    /// - Mínimo 8 caracteres
    /// - Al menos una minúscula
    /// - Al menos una mayúscula
    /// - Al menos un carácter especial
    /// </summary>
    /// <param name="password">Contraseña a validar</param>
    /// <returns>Tuple con bool (válida) y string (mensaje de error si aplica)</returns>
    private (bool isValid, string errorMessage) ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return (false, "La contraseña no puede estar vacía");
        }

        if (password.Length < 8)
        {
            return (false, "La contraseña debe tener al menos 8 caracteres");
        }

        // Verificar que tenga al menos una minúscula
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            return (false, "La contraseña debe contener al menos una letra minúscula");
        }

        // Verificar que tenga al menos una mayúscula
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            return (false, "La contraseña debe contener al menos una letra mayúscula");
        }

        // Verificar que tenga al menos un carácter especial
        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?~`]"))
        {
            return (false, "La contraseña debe contener al menos un carácter especial (!@#$%^&*()_+-=[]{}|;':\",./<>?~`)");
        }

        return (true, "");
    }

    #endregion

    #region Login y Register (métodos existentes)

    void HandleRegisterButton()
    {
        if (!isRegistering)
        {
            ShowRegisterFields();
            isRegistering = true;
            registerButton.text = "Confirmar Registro";
            ClearMessage();
        }
        else
        {
            Register();
        }
    }

    /// Mostrar los cuadros de registro
    void ShowRegisterFields()
    {
        nameInputContainer.style.display = DisplayStyle.Flex;
        roleContainer.style.display = DisplayStyle.Flex;
        nameInput.Focus();
    }

    /// Ocultar los cuadros de registro
    void HideRegisterFields()
    {
        nameInputContainer.style.display = DisplayStyle.None;
        roleContainer.style.display = DisplayStyle.None;
        registerButton.text = "Registrarse";
        isRegistering = false;
    }

    /// Inicio de sesiòn
    void Login()
    {
        string email = emailInput.value;
        string password = passwordInput.value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Todos los campos son obligatorios", true);
            return;
        }

        if (dbManager.usuarioService.CheckLogin(email, password))
        {
            ShowMessage("Inicio de sesión exitoso", false);
            Invoke("LoadCharacterSelect", 1.0f);
        }
        else
        {
            ShowMessage("Usuario o contraseña incorrectos", true);
        }
    }

    /// Cambiar de escena
    void LoadCharacterSelect()
    {
        SceneManager.LoadScene("CharacterSelect");
    }

    /// Registro del usuario (MODIFICADO con validación de contraseña)
    void Register()
    {
        string name = nameInput.value;
        string email = emailInput.value;
        string password = passwordInput.value;
        int selectedRoleId = GetSelectedRoleId();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Todos los campos son obligatorios", true);
            return;
        }

        // Validar formato de email básico
        if (!email.Contains("@") || !email.Contains("."))
        {
            ShowMessage("Ingresa un email válido", true);
            return;
        }

        // 🔒 NUEVA VALIDACIÓN DE CONTRASEÑA
        var (isPasswordValid, passwordError) = ValidatePassword(password);
        if (!isPasswordValid)
        {
            ShowMessage(passwordError, true);
            return;
        }

        // Si todas las validaciones pasan, proceder con el registro
        bool insertado = dbManager.usuarioService.Crear(email, password, name, selectedRoleId);

        if (insertado)
        {
            ShowMessage("Usuario registrado con éxito", false);
            HideRegisterFields();
            ClearInputs();
        }
        else
        {
            ShowMessage("Error al registrar usuario. El email podría estar ya registrado.", true);
        }
    }

    #endregion

    #region Cambio de Contraseña con Verificación

    /// Mostrar la pantalla de cambio de contraseña
    void ShowPasswordChangeScreen()
    {
        // Ocultar elementos de login/registro
        nameInputContainer.style.display = DisplayStyle.None;
        roleContainer.style.display = DisplayStyle.None;
        loginButton.style.display = DisplayStyle.None;
        registerButton.style.display = DisplayStyle.None;
        changePassButton.style.display = DisplayStyle.None;
        passwordInput.style.display = DisplayStyle.None;
        passwordInput.parent.Q<Label>().style.display = DisplayStyle.None; // Ocultar label de contraseña

        // Mostrar elementos de verificación
        verificationContainer.style.display = DisplayStyle.Flex;

        // Cambiar placeholder del email para indicar su propósito
        emailInput.SetValueWithoutNotify("");
        var emailLabel = emailInput.parent.Q<Label>();
        if (emailLabel != null)
            emailLabel.text = "Email para cambio de contraseña";

        isChangingPassword = true;
        ClearMessage();
        ShowMessage("Ingresa tu email y haz clic en 'Enviar Código'", false);
    }

    /// Enviar correo electrónico con el código de verificación
    void SendVerificationCode()
    {
        string email = emailInput.value;

        if (string.IsNullOrEmpty(email))
        {
            ShowMessage("Ingresa tu email", true);
            return;
        }

        // Validar formato de email básico
        if (!email.Contains("@") || !email.Contains("."))
        {
            ShowMessage("Ingresa un email válido", true);
            return;
        }

        ShowMessage("Enviando código...", false);

        bool sent = emailVerificationService.SendVerificationCode(email);

        if (sent)
        {
            ShowMessage("Código enviado a tu email. Revisa tu bandeja de entrada.", false);

            // Habilitar campos de código y nueva contraseña
            verificationCodeInput.SetEnabled(true);
            newPasswordInput.SetEnabled(true);
            verifyAndChangeButton.SetEnabled(true);
            sendCodeButton.text = "Reenviar Código";
        }
        else
        {
            ShowMessage("Error al enviar el código. Verifica que el email esté registrado.", true);
        }
    }

    /// Verifica el código y cambia la contraseña si coincide (MODIFICADO con validación)
    void VerifyCodeAndChangePassword()
    {
        string email = emailInput.value;
        string verificationCode = verificationCodeInput.value;
        string newPassword = newPasswordInput.value;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(verificationCode) || string.IsNullOrEmpty(newPassword))
        {
            ShowMessage("Todos los campos son obligatorios", true);
            return;
        }

        // 🔒 NUEVA VALIDACIÓN DE CONTRASEÑA
        var (isPasswordValid, passwordError) = ValidatePassword(newPassword);
        if (!isPasswordValid)
        {
            ShowMessage(passwordError, true);
            return;
        }

        ShowMessage("Verificando código...", false);

        // Verificar código y cambiar contraseña
        bool success = Cambiarcontraseña(email, verificationCode, newPassword);

        if (success)
        {
            ShowMessage("¡Contraseña cambiada exitosamente!", false);
            Invoke("BackToLogin", 2.0f); // Volver al login después de 2 segundos
        }
        else
        {
            ShowMessage("Código incorrecto, expirado o error al cambiar contraseña", true);
        }
    }

    /// Llama al método de usuarioService para cambiar la contraseña
    bool Cambiarcontraseña(string email, string verificationCode, string newPassword)
    {
        try
        {
            bool passwordChanged = dbManager.usuarioService.ChangePasswordWithVerification(email, verificationCode, newPassword);

            if (passwordChanged)
            {
                // Limpiar código de verificación
                emailVerificationService.ClearVerificationCode(email);
                Debug.Log("🔑 Contraseña cambiada exitosamente con verificación.");
                return true;
            }

            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError("❌ Error en el proceso de cambio de contraseña: " + e.Message);
            return false;
        }
    }

    /// Regresa a la pantalla de inicio de sesión
    void BackToLogin()
    {
        // Mostrar elementos de login
        loginButton.style.display = DisplayStyle.Flex;
        registerButton.style.display = DisplayStyle.Flex;
        changePassButton.style.display = DisplayStyle.Flex;
        passwordInput.style.display = DisplayStyle.Flex;
        passwordInput.parent.Q<Label>().style.display = DisplayStyle.Flex;

        // Ocultar elementos de verificación
        verificationContainer.style.display = DisplayStyle.None;
        HideRegisterFields();

        // Restaurar label del email
        var emailLabel = emailInput.parent.Q<Label>();
        if (emailLabel != null)
            emailLabel.text = "Email";

        // Reset estados
        isChangingPassword = false;
        sendCodeButton.text = "Enviar Código";

        // Deshabilitar campos hasta que se envíe código
        verificationCodeInput.SetEnabled(false);
        newPasswordInput.SetEnabled(false);
        verifyAndChangeButton.SetEnabled(false);

        ClearInputs();
        ClearMessage();
    }

    #endregion

    #region Helpers

    /// Enseña un mensaje en la pantalla
    void ShowMessage(string message, bool isError)
    {
        if (messageLabel != null)
        {
            messageLabel.text = message;
            messageLabel.RemoveFromClassList("success-message");
            messageLabel.RemoveFromClassList("error-message");

            if (isError)
            {
                messageLabel.AddToClassList("error-message");
            }
            else
            {
                messageLabel.AddToClassList("success-message");
            }

            messageLabel.style.display = DisplayStyle.Flex;
        }
    }

    void ClearMessage()
    {
        if (messageLabel != null)
        {
            messageLabel.text = "";
            messageLabel.style.display = DisplayStyle.None;
        }
    }

    void ClearInputs()
    {
        nameInput.value = "";
        emailInput.value = "";
        passwordInput.value = "";
        verificationCodeInput.value = "";
        newPasswordInput.value = "";
        if (roleDropdown != null && rolesFromDatabase.Count > 0)
            roleDropdown.index = 0;
    }

    // 🎵 Reproducir sonidos
    void PlayClick()
    {
        if (clickClip != null) audioSource.PlayOneShot(clickClip);
    }

    void PlayHover()
    {
        if (hoverClip != null) audioSource.PlayOneShot(hoverClip);
    }

    #endregion
}