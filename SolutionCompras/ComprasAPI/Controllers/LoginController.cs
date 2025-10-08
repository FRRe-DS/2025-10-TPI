using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ComprasAPI.Models;

namespace ComprasAPI.Controllers
{
    [ApiController]
    [Route("api/auth/login")]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // Lista de usuarios en memoria compartida
        public static List<RegisterRequest> Users = new List<RegisterRequest>();

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            try
            {
                // Validar campos obligatorios
                if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
                {
                    return BadRequest(new
                    {
                        error = "Email y Password son obligatorios",
                        code = "MISSING_FIELDS"
                    });
                }

                // Buscar usuario por email y contraseña
                var user = Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
                if (user == null)
                {
                    return Unauthorized(new
                    {
                        error = "Email o contraseña incorrectos",
                        code = "INVALID_CREDENTIALS"
                    });
                }

                var token = GenerateJwtToken(user);

                // 🔥 CORRECCIÓN: Usar FirstName y LastName en lugar de Nombre y Apellido
                return Ok(new
                {
                    token,
                    user = new
                    {
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        email = user.Email
                    }
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

        private string GenerateJwtToken(RegisterRequest user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Email),
                // 🔥 CORRECCIÓN: Agregar claims con FirstName y LastName
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Método opcional para agregar usuarios de prueba
        public static void AddTestUser(RegisterRequest user)
        {
            Users.Add(user);
        }

        // Método para debug: ver usuarios registrados
        [HttpGet("debug/users")]
        public IActionResult GetUsers()
        {
            var users = Users.Select(u => new {
                u.FirstName,
                u.LastName,
                u.Email
            }).ToList();

            return Ok(new
            {
                totalUsers = Users.Count,
                users
            });
        }
    }
}