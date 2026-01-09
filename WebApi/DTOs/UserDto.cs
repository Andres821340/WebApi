namespace WebApi.DTOs
{
    /// <summary>
    /// DTO para mostrar información de un usuario (sin contraseña)
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para solicitud de login
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// Nombre de usuario (requerido)
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario (requerido)
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO para respuesta de login exitoso
    /// </summary>
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public UserDto User { get; set; } = null!;
    }

    /// <summary>
    /// DTO para registrar un nuevo usuario
    /// </summary>
    public class RegisterRequestDto
    {
        /// <summary>
        /// Nombre de usuario único (requerido)
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Contraseña del usuario (requerido, mínimo 6 caracteres)
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Rol del usuario (User, Administrator)
        /// </summary>
        public string? Role { get; set; }
    }
}
