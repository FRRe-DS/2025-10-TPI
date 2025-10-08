using Microsoft.AspNetCore.Mvc;
using ComprasAPI.Models;

namespace ComprasAPI.Controllers
{
    [ApiController]
    [Route("api/auth/register")]
    public class RegisterController : ControllerBase
    {
        [HttpPost]
        public IActionResult Register([FromBody] RegisterRequest model)
        {
            try
            {
                // Validar que todos los campos estén presentes
                if (string.IsNullOrWhiteSpace(model.FirstName) ||
                    string.IsNullOrWhiteSpace(model.LastName) ||
                    string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.Password) ||
                    string.IsNullOrWhiteSpace(model.RepeatPassword))
                {
                    return BadRequest(new
                    {
                        error = "Todos los campos son obligatorios",
                        code = "MISSING_FIELDS"
                    });
                }

                // Validar formato de email
                if (!IsValidEmail(model.Email))
                {
                    return BadRequest(new
                    {
                        error = "El formato del email es inválido",
                        code = "INVALID_EMAIL"
                    });
                }

                // Validar que las contraseñas coincidan
                if (model.Password != model.RepeatPassword)
                {
                    return BadRequest(new
                    {
                        error = "Las contraseñas no coinciden",
                        code = "PASSWORD_MISMATCH"
                    });
                }

                // Validar fortaleza de contraseña (opcional)
                if (model.Password.Length < 6)
                {
                    return BadRequest(new
                    {
                        error = "La contraseña debe tener al menos 6 caracteres",
                        code = "WEAK_PASSWORD"
                    });
                }

                // Verificar si el email ya está registrado
                if (LoginController.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
                {
                    return Conflict(new
                    {
                        error = "El email ya está registrado",
                        code = "EMAIL_ALREADY_EXISTS"
                    });
                }

                // Agregar usuario a la lista estática de LoginController
                LoginController.Users.Add(model);

                return StatusCode(201, new
                {
                    message = "Usuario registrado exitosamente"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    code = "INTERNAL_ERROR"
                });
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}