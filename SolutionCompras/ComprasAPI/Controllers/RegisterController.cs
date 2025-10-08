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
            // Validar que todos los campos estén presentes
            if (string.IsNullOrWhiteSpace(model.Nombre) ||
                string.IsNullOrWhiteSpace(model.Apellido) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.Password))
            {
                return BadRequest("Todos los campos son obligatorios");
            }

            // Verificar si el email ya está registrado
            if (LoginController.Users.Any(u => u.Email == model.Email))
                return BadRequest("El email ya está registrado");

            // Agregar usuario a la lista estática de LoginController
            LoginController.Users.Add(model);

            return StatusCode(201, "Usuario registrado exitosamente");
        }
    }
}
