using ComprasAPI.Data;
using ComprasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace ComprasAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("keycloak-user")]
        [Authorize] // Este endpoint requiere token de Keycloak
        public async Task<IActionResult> GetOrCreateKeycloakUser()
        {
            try
            {
                // Extraer información del token de Keycloak
                var keycloakId = User.FindFirst("sub")?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
                var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? User.FindFirst("given_name")?.Value;
                var lastName = User.FindFirst(ClaimTypes.Surname)?.Value ?? User.FindFirst("family_name")?.Value;
                var username = User.FindFirst("preferred_username")?.Value;

                if (string.IsNullOrEmpty(keycloakId))
                {
                    return BadRequest(new { message = "Token de Keycloak invalido" });
                }

                // Buscar usuario por Keycloak ID
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

                if (user == null)
                {
                    // Si no existe, buscar por email
                    user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == email);

                    if (user != null)
                    {
                        // Migrar usuario existente a Keycloak
                        user.KeycloakId = keycloakId;
                        user.IsKeycloakUser = true;
                    }
                    else
                    {
                        // Crear nuevo usuario desde Keycloak
                        user = new User
                        {
                            KeycloakId = keycloakId,
                            Email = email,
                            FirstName = firstName ?? username?.Split('@')[0] ?? "Usuario",
                            LastName = lastName ?? "",
                            IsKeycloakUser = true,
                            CreatedAt = DateTime.Now
                        };

                        _context.Users.Add(user);
                    }

                    await _context.SaveChangesAsync();
                }

                // Devolver información del usuario
                return Ok(new
                {
                    message = "Autenticacion con Keycloak exitosa",
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        isKeycloakUser = user.IsKeycloakUser
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error en autenticacion con Keycloak", error = ex.Message });
            }
        }

        // Mantener login tradicional para usuarios no migrados (OPCIONAL)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsKeycloakUser);

            if (user == null)
                return Unauthorized(new { message = "Usuario no encontrado o usa Keycloak" });

            if (user.PasswordHash != HashPassword(request.Password))
                return Unauthorized(new { message = "Contraseña incorrecta" });

            return Ok(new
            {
                message = "Login exitoso",
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    isKeycloakUser = user.IsKeycloakUser
                }
            });
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

}