using ComprasAPI.Data;
using ComprasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/product")]
[Authorize] // Todos requieren autenticacion (Keycloak o tradicional)
public class ProductController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        try
        {
            // Obtener usuario actual desde Keycloak
            var keycloakId = User.FindFirst("sub")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            User user = null;

            if (!string.IsNullOrEmpty(keycloakId))
            {
                // Usuario de Keycloak
                user = await _context.Users
                    .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
            }

            // Si no encontramos usuario, podria ser usuario tradicional
            // o podriamos crear uno automaticamente

            var products = await _context.Product.ToListAsync();

            return Ok(new
            {
                User = user != null ? new { user.Email, user.FirstName, user.IsKeycloakUser } : null,
                Products = products,
                Count = products.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Error interno del servidor" });
        }
    }
}