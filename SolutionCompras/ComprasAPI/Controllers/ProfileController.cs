using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ComprasAPI.Models;

namespace ComprasAPI.Controllers
{
    [ApiController]
    [Route("api/user/profile")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        // Lista en memoria compartida - usamos email como clave ya que no tenemos UserId
        public static List<UserProfile> UserProfiles = new List<UserProfile>();
        private static int _nextId = 1;

        [HttpGet]
        [ProducesResponseType(typeof(UserProfile), 200)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 500)]
        public IActionResult GetUserProfile()
        {
            try
            {
                var userEmail = GetUserEmailFromToken();
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized(new { error = "No autorizado", code = "UNAUTHORIZED" });

                // Buscar perfil por email (simulamos UserId con el email)
                var profile = UserProfiles.FirstOrDefault(p => GetUserIdFromEmail(p.UserId) == userEmail);
                if (profile == null)
                    return NotFound(new { error = "Perfil no encontrado", code = "PROFILE_NOT_FOUND" });

                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", code = "INTERNAL_ERROR" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(object), 201)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public IActionResult CreateUserProfile([FromBody] UserProfileCreate request)
        {
            try
            {
                var userEmail = GetUserEmailFromToken();
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized(new { error = "No autorizado", code = "UNAUTHORIZED" });

                // Validar que el usuario existe
                var userExists = LoginController.Users.Any(u => u.Email == userEmail);
                if (!userExists)
                    return Unauthorized(new { error = "Usuario no encontrado", code = "USER_NOT_FOUND" });

                // Validar si el perfil ya existe
                var existingProfile = UserProfiles.Any(p => GetUserIdFromEmail(p.UserId) == userEmail);
                if (existingProfile)
                    return Conflict(new { error = "El perfil ya existe", code = "PROFILE_ALREADY_EXISTS" });

                // Validar DNI único
                var dniExists = UserProfiles.Any(p => p.Dni == request.Dni);
                if (dniExists)
                    return Conflict(new { error = "El DNI ya está registrado", code = "DNI_ALREADY_EXISTS" });

                // Como no tenemos UserId real, usamos un hash del email como "UserId"
                var profile = new UserProfile
                {
                    Id = _nextId++,
                    UserId = GenerateUserIdFromEmail(userEmail), // Convertimos email a "UserId"
                    Phone = request.Phone,
                    Dni = request.Dni,
                    BirthDate = request.BirthDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                UserProfiles.Add(profile);

                return Created(string.Empty, new { message = "Perfil creado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", code = "INTERNAL_ERROR" });
            }
        }

        [HttpPut]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(typeof(object), 400)]
        [ProducesResponseType(typeof(object), 401)]
        [ProducesResponseType(typeof(object), 404)]
        [ProducesResponseType(typeof(object), 409)]
        [ProducesResponseType(typeof(object), 500)]
        public IActionResult UpdateUserProfile([FromBody] UserProfileUpdate request)
        {
            try
            {
                var userEmail = GetUserEmailFromToken();
                if (string.IsNullOrEmpty(userEmail))
                    return Unauthorized(new { error = "No autorizado", code = "UNAUTHORIZED" });

                var existingProfile = UserProfiles.FirstOrDefault(p => GetUserIdFromEmail(p.UserId) == userEmail);
                if (existingProfile == null)
                    return NotFound(new { error = "Perfil no encontrado", code = "PROFILE_NOT_FOUND" });

                // Validar DNI único (excluyendo el perfil actual)
                var dniExists = UserProfiles.Any(p => p.Dni == request.Dni && GetUserIdFromEmail(p.UserId) != userEmail);
                if (dniExists)
                    return Conflict(new { error = "El DNI ya está registrado", code = "DNI_ALREADY_EXISTS" });

                existingProfile.Phone = request.Phone;
                existingProfile.Dni = request.Dni;
                existingProfile.BirthDate = request.BirthDate;
                existingProfile.UpdatedAt = DateTime.UtcNow;

                return Ok(new { message = "Perfil actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor", code = "INTERNAL_ERROR" });
            }
        }

        // Métodos auxiliares para simular UserId desde email
        private int GenerateUserIdFromEmail(string email)
        {
            // Usamos el hash del email como "UserId" simulado
            return Math.Abs(email.GetHashCode());
        }

        private string GetUserIdFromEmail(int userId)
        {
            // Buscar el email que corresponde a este UserId simulado
            var user = LoginController.Users.FirstOrDefault(u => GenerateUserIdFromEmail(u.Email) == userId);
            return user?.Email ?? string.Empty;
        }

        private string? GetUserEmailFromToken()
        {
            var emailClaim = User.FindFirst(ClaimTypes.Name) ?? User.FindFirst(ClaimTypes.NameIdentifier);
            return emailClaim?.Value;
        }

        // Método para agregar perfiles de prueba
        public static void AddTestProfile(UserProfile profile)
        {
            UserProfiles.Add(profile);
        }
    }
}