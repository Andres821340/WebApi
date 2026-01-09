using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Models;
using WebApi.Services;
using WebApi.DTOs;

namespace WebApi.Controllers
{
    /// <summary>
    /// Controlador para autenticación y registro de usuarios
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly AppDbContext _context;

        public AuthController(JwtService jwtService, AppDbContext context)
        {
            _jwtService = jwtService;
            _context = context;
        }

        /// <summary>
        /// Inicia sesión y obtiene un token JWT
        /// </summary>
        /// <param name="request">Credenciales de usuario</param>
        /// <returns>Token JWT y datos del usuario</returns>
        /// <response code="200">Login exitoso - Retorna token JWT</response>
        /// <response code="400">Datos de entrada inválidos</response>
        /// <response code="401">Credenciales inválidas</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(ApiResponse<object>.Fail("Username y Password son requeridos"));
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Credenciales inválidas"));
            }

            var token = _jwtService.GenerateToken(user);
            var expiration = DateTime.Now.AddHours(1);

            var response = new LoginResponseDto
            {
                Token = token,
                Expiration = expiration,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role
                }
            };

            return Ok(ApiResponse<LoginResponseDto>.Ok(response, "Login exitoso"));
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema
        /// </summary>
        /// <param name="request">Datos del nuevo usuario</param>
        /// <returns>Información del usuario registrado</returns>
        /// <response code="200">Usuario registrado exitosamente</response>
        /// <response code="400">Datos inválidos o usuario ya existe</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(ApiResponse<object>.Fail("Username y Password son requeridos"));
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(ApiResponse<object>.Fail("La contraseña debe tener al menos 6 caracteres"));
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (existingUser != null)
            {
                return BadRequest(ApiResponse<object>.Fail("El nombre de usuario ya existe"));
            }

            var user = new UserModel
            {
                Username = request.Username,
                Password = request.Password,
                Email = request.Email ?? string.Empty,
                Role = request.Role ?? "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(ApiResponse<UserDto>.Ok(userDto, "Usuario registrado exitosamente"));
        }

        /// <summary>
        /// Obtiene la lista de todos los usuarios (Solo Administradores)
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        /// <response code="200">Lista de usuarios</response>
        /// <response code="401">No autorizado</response>
        /// <response code="403">Acceso denegado - Solo administradores</response>
        [HttpGet("users")]
        [Authorize(Roles = "Administrator")]
        [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(ApiResponse<List<UserDto>>.Ok(users, "Usuarios obtenidos exitosamente"));
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado
        /// </summary>
        /// <returns>Datos del usuario actual</returns>
        /// <response code="200">Datos del usuario</response>
        /// <response code="401">No autorizado</response>
        [HttpGet("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(ApiResponse<object>.Fail("Usuario no autenticado"));
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return NotFound(ApiResponse<object>.Fail("Usuario no encontrado"));
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(ApiResponse<UserDto>.Ok(userDto));
        }
    }
}
