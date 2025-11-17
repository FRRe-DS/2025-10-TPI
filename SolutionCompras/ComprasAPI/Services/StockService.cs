using System.Text.Json;
using ComprasAPI.Models.DTOs;

namespace ComprasAPI.Services
{
    public class StockService : IStockService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StockService> _logger;

        public StockService(HttpClient httpClient, ILogger<StockService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<ProductoStock>> GetAllProductsAsync()
        {
            try
            {
                _logger.LogInformation("Obteniendo productos desde Stock API...");

                var response = await _httpClient.GetAsync("/productos");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var productos = JsonSerializer.Deserialize<List<ProductoStock>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation($"✅ Obtenidos {productos?.Count ?? 0} productos");
                return productos ?? new List<ProductoStock>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Stock API no disponible - Usando datos de prueba");

                // ✅ DATOS DE PRUEBA cuando Stock API no está disponible
                return GetProductosDePrueba();
            }
        }

        public async Task<ProductoStock> GetProductByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Obteniendo producto {id} desde Stock API...");

                var response = await _httpClient.GetAsync($"/productos/{id}");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ProductoStock>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"❌ Stock API no disponible - Buscando producto {id} en datos de prueba");

                // ✅ BUSCAR en datos de prueba
                var productos = GetProductosDePrueba();
                return productos.FirstOrDefault(p => p.Id == id);
            }
        }

        // ✅ MÉTODO CON DATOS DE PRUEBA (CORREGIDO CON 'M' EN DECIMALES)
        private List<ProductoStock> GetProductosDePrueba()
        {
            return new List<ProductoStock>
            {
                new ProductoStock
                {
                    Id = 1,
                    Nombre = "Laptop Gaming",
                    Descripcion = "Laptop para gaming de alta performance",
                    Precio = 1500.00M,  // ← AGREGAR 'M'
                    StockDisponible = 10,
                    PesoKg = 2.5M,      // ← AGREGAR 'M'
                    Dimensiones = new Dimensiones { LargoCm = 35.0M, AnchoCm = 25.0M, AltoCm = 2.5M }, // ← AGREGAR 'M'
                    Ubicacion = new UbicacionAlmacen
                    {
                        Street = "Av. Siempre Viva 123",
                        City = "Resistencia",
                        State = "Chaco",
                        PostalCode = "H3500ABC",
                        Country = "AR"
                    },
                    Categorias = new List<Categoria>
                    {
                        new Categoria { Id = 1, Nombre = "Electrónica", Descripcion = "Productos electrónicos" }
                    }
                },
                new ProductoStock
                {
                    Id = 2,
                    Nombre = "Mouse Inalámbrico",
                    Descripcion = "Mouse ergonómico inalámbrico",
                    Precio = 45.50M,    // ← AGREGAR 'M'
                    StockDisponible = 25,
                    PesoKg = 0.2M,      // ← AGREGAR 'M'
                    Dimensiones = new Dimensiones { LargoCm = 12.0M, AnchoCm = 6.0M, AltoCm = 3.0M }, // ← AGREGAR 'M'
                    Ubicacion = new UbicacionAlmacen
                    {
                        Street = "Av. Vélez Sársfield 456",
                        City = "Resistencia",
                        State = "Chaco",
                        PostalCode = "H3500XYZ",
                        Country = "AR"
                    },
                    Categorias = new List<Categoria>
                    {
                        new Categoria { Id = 1, Nombre = "Electrónica", Descripcion = "Productos electrónicos" },
                        new Categoria { Id = 2, Nombre = "Accesorios", Descripcion = "Accesorios para computadora" }
                    }
                },
                new ProductoStock
                {
                    Id = 3,
                    Nombre = "Teclado Mecánico",
                    Descripcion = "Teclado mecánico RGB",
                    Precio = 120.00M,   // ← AGREGAR 'M'
                    StockDisponible = 15,
                    PesoKg = 1.1M,      // ← AGREGAR 'M'
                    Dimensiones = new Dimensiones { LargoCm = 44.0M, AnchoCm = 14.0M, AltoCm = 3.0M }, // ← AGREGAR 'M'
                    Ubicacion = new UbicacionAlmacen
                    {
                        Street = "Calle Falsa 123",
                        City = "Resistencia",
                        State = "Chaco",
                        PostalCode = "H3500DEF",
                        Country = "AR"
                    },
                    Categorias = new List<Categoria>
                    {
                        new Categoria { Id = 1, Nombre = "Electrónica", Descripcion = "Productos electrónicos" }
                    }
                }
            };
        }
    }
}