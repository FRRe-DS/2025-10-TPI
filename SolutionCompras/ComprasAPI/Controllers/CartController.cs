using ComprasAPI.Data;
using ComprasAPI.Models;
using ComprasAPI.Models.DTOs;
using ComprasAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ComprasAPI.Controllers
{
    [ApiController]
    [Route("api/shopcart")]
    public class CartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IStockService _stockService;
        private readonly ILogger<CartController> _logger;

        public CartController(ApplicationDbContext context, IStockService stockService, ILogger<CartController> logger)
        {
            _context = context;
            _stockService = stockService;
            _logger = logger;
        }

        // GET: api/shopcart
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                _logger.LogInformation("Obteniendo carrito del usuario...");

                var userId = await GetCurrentUserId();
                _logger.LogInformation($"UserId obtenido: {userId}");

                if (userId == null)
                {
                    _logger.LogWarning("UserId es null - Usuario no autorizado o no encontrado en BD local");
                    return Unauthorized(new { error = "No autorizado", code = "UNAUTHORIZED" });
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    _logger.LogInformation("Creando carrito vacío para nuevo usuario");
                    // Crear carrito vacío si no existe
                    cart = new Cart { UserId = userId.Value, Items = new List<CartItem>() };
                    return Ok(new CartDto
                    {
                        Id = 0,
                        Total = 0,
                        UserId = userId.Value,
                        Items = new List<CartItemDto>()
                    });
                }

                // Calcular total actualizado
                cart.Total = cart.Items.Sum(item => item.Product.Price * item.Quantity);
                _logger.LogInformation($"Carrito obtenido: {cart.Items.Count} items, Total: {cart.Total}");

                // USAR DTO PARA EVITAR CICLOS
                var cartDto = new CartDto
                {
                    Id = cart.Id,
                    Total = cart.Total,
                    UserId = cart.UserId,
                    Items = cart.Items.Select(item => new CartItemDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Product = new ProductDto
                        {
                            Id = item.Product.Id,
                            Name = item.Product.Name,
                            Description = item.Product.Description,
                            Price = item.Product.Price,
                            Stock = item.Product.Stock,
                            Category = item.Product.Category
                        }
                    }).ToList()
                };

                return Ok(cartDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener carrito");
                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    code = "INTERNAL_ERROR"
                });
            }
        }

        // POST: api/shopcart
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("🔒 INICIANDO AddToCart CON TRANSACCIÓN...");
                _logger.LogInformation($"📦 Producto: {request.ProductId}, Cantidad: {request.Quantity}");

                var userId = await GetCurrentUserId();
                _logger.LogInformation($"UserId obtenido: {userId}");

                if (userId == null)
                {
                    await transaction.RollbackAsync();
                    return Unauthorized(new { error = "No autorizado", code = "UNAUTHORIZED" });
                }

                // 1. Verificar producto en Stock API
                _logger.LogInformation($"Verificando producto {request.ProductId} en Stock API...");
                var stockProduct = await _stockService.GetProductByIdAsync(request.ProductId);
                if (stockProduct == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound(new { error = "Producto no encontrado", code = "PRODUCT_NOT_FOUND" });
                }

                // 2. Verificar stock
                if (stockProduct.StockDisponible < request.Quantity)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new
                    {
                        error = "Stock insuficiente",
                        code = "INSUFFICIENT_STOCK",
                        available = stockProduct.StockDisponible
                    });
                }

                // 3. Obtener carrito CON RECARGA EXPLÍCITA
                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                // 🔍 DEBUG: Verificar qué items tiene el carrito
                _logger.LogInformation($"🔍 Carrito ID: {cart?.Id}, Items count: {cart?.Items?.Count}");
                if (cart?.Items != null)
                {
                    foreach (var item in cart.Items)
                    {
                        _logger.LogInformation($"🔍 Item ID: {item.Id}, ProductId: {item.ProductId}, Quantity: {item.Quantity}");
                    }
                }

                if (cart == null)
                {
                    cart = new Cart { UserId = userId.Value };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"🆕 Carrito creado: {cart.Id}");
                }

                // 4. BUSCAR ITEM EXISTENTE - MÁS ROBUSTO
                CartItem existingItem = null;
                if (cart.Items != null)
                {
                    existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
                }

                _logger.LogInformation($"🔍 ExistingItem encontrado: {existingItem != null}");
                if (existingItem != null)
                {
                    _logger.LogInformation($"🔍 ExistingItem details - ID: {existingItem.Id}, ProductId: {existingItem.ProductId}, Quantity: {existingItem.Quantity}");
                }

                // 5. AGREGAR O ACTUALIZAR ITEM
                if (existingItem != null)
                {
                    // ✅ ACTUALIZAR ITEM EXISTENTE
                    var newQuantity = existingItem.Quantity + request.Quantity;

                    if (newQuantity > stockProduct.StockDisponible)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest(new
                        {
                            error = "Stock insuficiente",
                            code = "INSUFFICIENT_STOCK",
                            available = stockProduct.StockDisponible
                        });
                    }

                    existingItem.Quantity = newQuantity;
                    _logger.LogInformation($"✅ ITEM ACTUALIZADO - ID: {existingItem.Id}, Nueva cantidad: {newQuantity}");
                }
                else
                {
                    // ✅ CREAR NUEVO ITEM (SOLO SI NO EXISTE)
                    // Primero buscar/crear producto local
                    var localProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == request.ProductId);

                    if (localProduct == null)
                    {
                        localProduct = new Product
                        {
                            Id = stockProduct.Id,
                            Name = stockProduct.Nombre,
                            Description = stockProduct.Descripcion,
                            Price = stockProduct.Precio,
                            Stock = stockProduct.StockDisponible,
                            Category = stockProduct.Categorias?.FirstOrDefault()?.Nombre ?? "General"
                        };
                        _context.Products.Add(localProduct);
                        await _context.SaveChangesAsync();
                    }

                    var newCartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        Product = localProduct
                    };

                    // Asegurar que la colección Items esté inicializada
                    if (cart.Items == null)
                    {
                        cart.Items = new List<CartItem>();
                    }

                    cart.Items.Add(newCartItem);
                    _logger.LogInformation($"🆕 NUEVO ITEM CREADO - ProductId: {request.ProductId}, Quantity: {request.Quantity}");
                }

                // 6. Actualizar total
                cart.Total = cart.Items.Sum(item =>
                {
                    var product = item.Product ?? _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                    return product?.Price * item.Quantity ?? 0;
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"🎯 TRANSACCIÓN EXITOSA - Carrito ID: {cart.Id}, Total Items: {cart.Items.Count}");

                return Ok(new
                {
                    message = "Producto agregado al carrito",
                    cartId = cart.Id,
                    total = cart.Total,
                    itemsCount = cart.Items.Count
                });
            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, " Error al conectar con Stock API");
                return StatusCode(502, new { error = "Servicio Stock no disponible", code = "STOCK_SERVICE_UNAVAILABLE" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "💥 Error en AddToCart");
                return StatusCode(500, new { error = "Error interno del servidor", code = "INTERNAL_ERROR" });
            }
        }

        // PUT: api/shopcart
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartRequest request)
        {
            try
            {
                _logger.LogInformation(" Actualizando item del carrito...");

                var userId = await GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning(" UserId es null - Usuario no autorizado");
                    return Unauthorized(new { error = "No autorizado", code = "UNAUTHORIZED" });
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    _logger.LogWarning(" Carrito no encontrado");
                    return NotFound(new { error = "Carrito no encontrado", code = "CART_NOT_FOUND" });
                }

                var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
                if (cartItem == null)
                {
                    _logger.LogWarning($" Producto {request.ProductId} no encontrado en el carrito");
                    return NotFound(new { error = "Producto no encontrado en el carrito", code = "CART_ITEM_NOT_FOUND" });
                }

                // Verificar stock si se aumenta la cantidad
                if (request.Quantity > cartItem.Quantity)
                {
                    var stockProduct = await _stockService.GetProductByIdAsync(request.ProductId);
                    if (stockProduct.StockDisponible < request.Quantity)
                    {
                        _logger.LogWarning($" Stock insuficiente al actualizar. Solicitado: {request.Quantity}, Disponible: {stockProduct.StockDisponible}");
                        return BadRequest(new
                        {
                            error = "Stock insuficiente",
                            code = "INSUFFICIENT_STOCK",
                            available = stockProduct.StockDisponible
                        });
                    }
                }

                if (request.Quantity <= 0)
                {
                    // Eliminar item si cantidad es 0 o negativa
                    cart.Items.Remove(cartItem);
                    _context.CartItems.Remove(cartItem);
                    _logger.LogInformation($" Producto {cartItem.Product.Name} removido del carrito");
                }
                else
                {
                    cartItem.Quantity = request.Quantity;
                    _logger.LogInformation($" Producto {cartItem.Product.Name} actualizado a {request.Quantity} unidades");
                }

                // Actualizar total
                cart.Total = cart.Items.Sum(item => item.Product.Price * item.Quantity);

                await _context.SaveChangesAsync();

                _logger.LogInformation($" Carrito actualizado. Total: {cart.Total}");

                return Ok(new { message = "Carrito actualizado", total = cart.Total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error al actualizar carrito");
                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    code = "INTERNAL_ERROR"
                });
            }
        }

        // DELETE: api/shopcart/{productId}
        [HttpDelete("{productId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            try
            {
                _logger.LogInformation($" Removiendo producto {productId} del carrito...");

                var userId = await GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning(" UserId es null - Usuario no autorizado");
                    return Unauthorized(new { error = "No autorizado", code = "UNAUTHORIZED" });
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null)
                {
                    _logger.LogWarning(" Carrito no encontrado");
                    return NotFound(new { error = "Carrito no encontrado", code = "CART_NOT_FOUND" });
                }

                var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (cartItem == null)
                {
                    _logger.LogWarning($" Producto {productId} no encontrado en el carrito");
                    return NotFound(new { error = "Producto no encontrado en el carrito", code = "CART_ITEM_NOT_FOUND" });
                }

                cart.Items.Remove(cartItem);
                _context.CartItems.Remove(cartItem);

                // Actualizar total
                cart.Total = cart.Items.Sum(item => item.Product.Price * item.Quantity);

                await _context.SaveChangesAsync();

                _logger.LogInformation($" Producto {productId} removido. Nuevo total: {cart.Total}");

                return Ok(new { message = "Producto removido del carrito", total = cart.Total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error al remover producto del carrito");
                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    code = "INTERNAL_ERROR"
                });
            }
        }

        // DELETE: api/shopcart
        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                _logger.LogInformation(" Vaciando carrito...");

                var userId = await GetCurrentUserId();
                if (userId == null)
                {
                    _logger.LogWarning(" UserId es null - Usuario no autorizado");
                    return Unauthorized(new { error = "No autorizado", code = "UNAUTHORIZED" });
                }

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.Items.Any())
                {
                    _logger.LogInformation(" Carrito ya está vacío");
                    return Ok(new { message = "Carrito ya está vacío" });
                }

                _logger.LogInformation($" Eliminando {cart.Items.Count} items del carrito");
                _context.CartItems.RemoveRange(cart.Items);
                cart.Items.Clear();
                cart.Total = 0;

                await _context.SaveChangesAsync();

                _logger.LogInformation(" Carrito vaciado exitosamente");

                return Ok(new { message = "Carrito vaciado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error al vaciar carrito");
                return StatusCode(500, new
                {
                    error = "Error interno del servidor",
                    code = "INTERNAL_ERROR"
                });
            }
        }

        // 🔧 MÉTODO ACTUALIZADO - OBTENER USERID DE BASE DE DATOS LOCAL
        private async Task<int?> GetCurrentUserId()
        {
            try
            {
                _logger.LogInformation(" Buscando userId en base de datos local...");

                // 1. Obtener el email del token de Keycloak
                var email = User.FindFirst(ClaimTypes.Email)?.Value
                           ?? User.FindFirst("email")?.Value
                           ?? User.FindFirst("preferred_username")?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning(" No se encontró email en el token");
                    return null;
                }

                _logger.LogInformation($" Email del usuario: {email}");

                // 2. Buscar el usuario en tu base de datos por email
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    _logger.LogWarning($" Usuario con email {email} no encontrado en base de datos local");

                    // Opcional: Crear usuario automáticamente si no existe
                    _logger.LogInformation(" Creando usuario automáticamente...");
                    user = new User
                    {
                        Email = email,
                        FirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "Usuario",
                        LastName = User.FindFirst(ClaimTypes.Surname)?.Value ?? "Keycloak",
                        PasswordHash = "keycloak_user", // Placeholder
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($" Usuario creado automáticamente: {user.Id}");
                }

                _logger.LogInformation($" UserId de base de datos local: {user.Id}");
                return user.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error al obtener userId de base de datos local");


                _logger.LogError("❌ No se pudo obtener userId - sin fallback a modo test");
                return null;
            }
        }

        // 🔍 ENDPOINT DEBUG - PARA VERIFICAR EL TOKEN Y USER
        [HttpGet("debug-token")]
        [Authorize]
        public async Task<IActionResult> DebugToken()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var userId = await GetCurrentUserId();

            return Ok(new
            {
                message = " Debug de Token y Usuario",
                userIdFromDatabase = userId,
                email = User.FindFirst(ClaimTypes.Email)?.Value,
                preferred_username = User.FindFirst("preferred_username")?.Value,
                keycloakUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                allClaims = claims
            });
        }





    // Modelos para las requests
    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
}