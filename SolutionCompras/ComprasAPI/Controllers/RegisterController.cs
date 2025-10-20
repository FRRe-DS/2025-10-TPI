using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ComprasAPI.Data;
using ComprasAPI.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace ComprasAPI.Controllers
{
    [ApiController]
    [Route("api/auth/register")]
    public class RegisterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RegisterController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (request.Password != request.RepeatPassword)
                return BadRequest(new { error = "Las contraseñas no coinciden" });

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return Conflict(new { error = "El correo ya está registrado" });

            // 1️⃣ Crear usuario en Keycloak
            var token = await GetAdminToken();
            if (token == null)
                return StatusCode(500, new { error = "No se pudo obtener el token de administrador de Keycloak" });

            var client = _httpClientFactory.CreateClient();

            var userPayload = new
            {
                username = request.Email,
                email = request.Email,
                enabled = true,
                firstName = request.FirstName,
                lastName = request.LastName,
                credentials = new[]
                {
                    new { type = "password", value = request.Password, temporary = false }
                }
            };

            var json = JsonSerializer.Serialize(userPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var keycloakUrl = "http://localhost:8080/admin/realms/ds-2025-realm/users";
            var response = await client.PostAsync(keycloakUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { error = "Error al crear usuario en Keycloak", details = errorBody });
            }

            // 2️⃣ Guardar en base de datos local
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registrado correctamente" });
        }

        // 🔐 Obtener token admin de Keycloak
        private async Task<string?> GetAdminToken()
        {
            var client = _httpClientFactory.CreateClient();
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "admin-cli"),
                new KeyValuePair<string, string>("username", "admin"),     // Cambiar si tu usuario admin tiene otro nombre
                new KeyValuePair<string, string>("password", "ds2025"),  // ⚠️ Cambia esta contraseña
                new KeyValuePair<string, string>("grant_type", "password")
            });

            var response = await client.PostAsync("http://localhost:8080/realms/master/protocol/openid-connect/token", data);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
