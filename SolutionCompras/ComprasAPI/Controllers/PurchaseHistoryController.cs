using ComprasAPI.Data;
using ComprasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Security.Claims;

namespace ComprasAPI.Controllers
{
    [ApiController]
    [Route("api/purchases")]
    [Authorize]
    public class PurchaseHistoryController : ControllerBase
    {
        private readonly ILogger<PurchaseHistoryController> _logger;
        private readonly ApplicationDbContext _context;

        public PurchaseHistoryController(
            ILogger<PurchaseHistoryController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: api/purchases/history
        [HttpGet("history")]
        public async Task<IActionResult> GetPurchaseHistory()
        {
            try
            {
                _logger.LogInformation("📋 Obteniendo historial de compras...");

                // 1. Obtener userId REAL del usuario autenticado
                var userId = await GetCurrentUserId();

                if (userId == null)
                {
                    _logger.LogWarning("Usuario no encontrado en BD local");
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Usuario no autorizado"
                    });
                }

                _logger.LogInformation($"Buscando compras para userId: {userId}");

                // 2. Conexión DIRECTA a MySQL de logística
                string connectionString = "server=localhost;port=3308;database=apidepapas;user=ApiUser;password=ApiDePapas_G6_Logistica";

                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                // 3. Consulta SQL REAL a la tabla Shippings
                string query = @"
                    SELECT 
                        shipping_id,
                        order_id,
                        status,
                        tracking_number,
                        total_cost,
                        currency,
                        DATE_FORMAT(estimated_delivery_at, '%d/%m/%Y') as estimated_delivery,
                        DATE_FORMAT(created_at, '%d/%m/%Y') as order_date
                    FROM Shippings 
                    WHERE user_id = @UserId 
                    ORDER BY created_at DESC";

                using var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId.Value);

                using var reader = await command.ExecuteReaderAsync();

                var purchases = new List<object>();
                int count = 0;

                while (reader.Read())
                {
                    count++;
                    purchases.Add(new
                    {
                        shippingId = reader.GetInt32("shipping_id"),
                        orderId = reader.GetInt32("order_id"),
                        status = reader.GetInt32("status") switch
                        {
                            0 => "Creado",          // created
                            1 => "Reservado",       // reserved
                            2 => "En tránsito",     // in_transit
                            3 => "Entregado",       // delivered
                            4 => "Cancelado",       // cancelled
                            5 => "En distribución", // indistribution
                            6 => "Llegó al centro", // arrived
                            _ => "Desconocido"
                        },
                        trackingNumber = reader.IsDBNull(reader.GetOrdinal("tracking_number"))
                            ? ""
                            : reader.GetString("tracking_number"),
                        //carrierName = reader.GetString("carrier_name"),
                        totalCost = reader.GetDecimal("total_cost"),
                        currency = reader.GetString("currency"),
                        estimatedDelivery = reader.GetString("estimated_delivery"),
                        orderDate = reader.GetString("order_date")
                    });
                }

                _logger.LogInformation($"✅ Encontradas {count} compras para userId: {userId}");

                return Ok(new
                {
                    success = true,
                    data = purchases
                });
            }
            catch (MySqlException mysqlEx)
            {
                _logger.LogError(mysqlEx, "❌ Error de conexión MySQL");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error conectando con la base de datos de logística"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error en historial de compras");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        // 🔥 MÉTODO REAL - IGUAL QUE EN TU CART CONTROLLER
        private async Task<int?> GetCurrentUserId()
        {
            try
            {
                // 1. Obtener email del token (igual que en CartController)
                var email = User.FindFirst(ClaimTypes.Email)?.Value
                           ?? User.FindFirst("email")?.Value
                           ?? User.FindFirst("preferred_username")?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("No se encontró email en el token");
                    return null;
                }

                _logger.LogInformation($"Buscando usuario por email: {email}");

                // 2. Buscar usuario en TU Base de Datos Local
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    _logger.LogWarning($"Usuario con email {email} no encontrado en BD local");

                    // Crear usuario automáticamente (igual que en CartController)
                    _logger.LogInformation("Creando usuario automáticamente...");
                    user = new User
                    {
                        Email = email,
                        FirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Usuario",
                        LastName = User.FindFirst(ClaimTypes.Surname)?.Value ?? "Keycloak",
                        PasswordHash = "keycloak_user",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Usuario creado: Id={user.Id}");
                }

                _logger.LogInformation($"UserId obtenido: {user.Id}");
                return user.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener userId");
                return null;
            }
        }
    }
}
