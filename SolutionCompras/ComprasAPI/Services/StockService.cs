using ComprasAPI.Models.DTOs;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ComprasAPI.Services
{
    public class StockService : IStockService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StockService> _logger;
        private readonly IConfiguration _configuration;
        private string _cachedToken;
        private DateTime _tokenExpiry;

        public StockService(HttpClient httpClient, ILogger<StockService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        /*private async Task<string> GetAccessTokenAsync()
        {
            // Si tenemos un token válido en caché, lo usamos
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _cachedToken;
            }

            try
            {
                _logger.LogInformation("🔑 Obteniendo token de Keycloak...");

                var tokenEndpoint = _configuration["StockApi:TokenEndpoint"];
                var clientId = _configuration["StockApi:ClientId"];
                var clientSecret = _configuration["StockApi:ClientSecret"];

                var tokenRequest = new List<KeyValuePair<string, string>>
                {
                    new("grant_type", "client_credentials"),
                    new("client_id", clientId),
                    new("client_secret", clientSecret)
                };

                var content = new FormUrlEncodedContent(tokenRequest);
                var response = await _httpClient.PostAsync(tokenEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("❌ Error obteniendo token de Keycloak: {StatusCode}", response.StatusCode);
                    throw new Exception($"Error obteniendo token: {response.StatusCode}");
                }

                var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>();
                _cachedToken = tokenResponse.AccessToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Restamos 60 segundos de margen

                _logger.LogInformation("✅ Token de Keycloak obtenido exitosamente");
                return _cachedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error crítico obteniendo token de Keycloak");
                throw;
            }
        }
        
        private async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(HttpMethod method, string endpoint)
        {
            var token = await GetAccessTokenAsync();
            var request = new HttpRequestMessage(method, endpoint);

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("Accept", "application/json");

            return request;
        }
        */
        private async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(HttpMethod method, string endpoint)
        {
            var token = await GetAccessTokenAsync();

            // ✅ USAR URL desde configuración
            var baseUrl = _configuration["StockApi:BaseUrl"]; // "http://localhost:3000"
            var absoluteUrl = $"{baseUrl}{endpoint}";
            var request = new HttpRequestMessage(method, absoluteUrl);

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("Accept", "application/json");

            _logger.LogInformation("🔍 DEBUG: URL absoluta: {Url}", absoluteUrl);
            return request;
        }


        private async Task<string> GetAccessTokenAsync()
        {
            // Si tenemos un token válido en caché, lo usamos
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            {
                _logger.LogInformation("🔑 Usando token en caché");
                return _cachedToken;
            }

            try
            {
                _logger.LogInformation("🔑 Obteniendo NUEVO token de Keycloak...");

                var tokenEndpoint = _configuration["StockApi:TokenEndpoint"];
                var clientId = _configuration["StockApi:ClientId"];
                var clientSecret = _configuration["StockApi:ClientSecret"];

                _logger.LogInformation("🔍 DEBUG: TokenEndpoint: {Endpoint}", tokenEndpoint);
                _logger.LogInformation("🔍 DEBUG: ClientId: {ClientId}", clientId);

                var tokenRequest = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", clientId),
            new("client_secret", clientSecret)
        };

                var content = new FormUrlEncodedContent(tokenRequest);
                var response = await _httpClient.PostAsync(tokenEndpoint, content);

                _logger.LogInformation("🔍 DEBUG: Token response status: {Status}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Error obteniendo token de Keycloak: {StatusCode} - {Content}",
                        response.StatusCode, errorContent);
                    throw new Exception($"Error obteniendo token: {response.StatusCode}");
                }

                var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>();
                _cachedToken = tokenResponse.AccessToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

                _logger.LogInformation("✅ Token de Keycloak obtenido exitosamente. Longitud: {Length}", _cachedToken.Length);
                return _cachedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error crítico obteniendo token de Keycloak");
                throw;
            }
        }
        /*public async Task<List<ProductoStock>> GetAllProductsAsync()
        {
            try
            {
                _logger.LogInformation("📦 Obteniendo productos desde Stock API...");

                var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, "/productos");
                var response = await _httpClient.SendAsync(request);

                _logger.LogInformation("📡 Response Status: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("❌ Error de Stock API: {StatusCode} - {Reason}",
                        response.StatusCode, response.ReasonPhrase);
                    throw new HttpRequestException($"Stock API returned {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("✅ Respuesta recibida de Stock API");

                var productos = JsonSerializer.Deserialize<List<ProductoStock>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation($"✅ Obtenidos {productos?.Count ?? 0} productos reales de Stock API");
                return productos ?? new List<ProductoStock>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Stock API no disponible - Usando datos de prueba");
                return GetProductosDePrueba();
            }
        }
        

        public async Task<List<ProductoStock>> GetAllProductsAsync()
        {
            try
            {
                _logger.LogInformation("📦 Obteniendo productos desde Stock API...");

                var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, "/productos");

                // ✅ DEBUG: Verificar el token y headers
                _logger.LogInformation("🔍 DEBUG: Request URI: {Uri}", request.RequestUri);
                _logger.LogInformation("🔍 DEBUG: Tiene Authorization header: {HasAuth}",
                    request.Headers.Authorization != null);
                if (request.Headers.Authorization != null)
                {
                    _logger.LogInformation("🔍 DEBUG: Token: {Token}",
                        request.Headers.Authorization.Parameter?.Substring(0, Math.Min(20, request.Headers.Authorization.Parameter.Length)) + "...");
                }

                var response = await _httpClient.SendAsync(request);

                _logger.LogInformation("📡 Response Status: {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Error de Stock API: {StatusCode} - {Reason}",
                        response.StatusCode, response.ReasonPhrase);
                    _logger.LogError("❌ Error details: {Content}", errorContent);
                    throw new HttpRequestException($"Stock API returned {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("✅ Respuesta recibida de Stock API");

                var productos = JsonSerializer.Deserialize<List<ProductoStock>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation($"✅ Obtenidos {productos?.Count ?? 0} productos reales de Stock API");
                return productos ?? new List<ProductoStock>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Stock API no disponible - Usando datos de prueba");
                return GetProductosDePrueba();
            }
        }
        */
        public async Task<List<ProductoStock>> GetAllProductsAsync()
        {
            try
            {
                _logger.LogInformation("🔍 Obteniendo productos desde Stock API...");

                var token = await GetAccessTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:3000/productos");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ Error HTTP: {response.StatusCode} - {errorContent}");
                    throw new HttpRequestException($"Error: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"📦 Respuesta recibida, longitud: {content.Length} caracteres");

                // La API de Stock devuelve { "data": [ ...productos... ] }
                var responseWrapper = JsonSerializer.Deserialize<StockApiResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (responseWrapper?.Data != null)
                {
                    _logger.LogInformation($"✅ Obtenidos {responseWrapper.Data.Count} productos REALES de Stock API");
                    return responseWrapper.Data;
                }
                else
                {
                    _logger.LogWarning("❌ No se encontraron productos en la respuesta");
                    return new List<ProductoStock>();
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "❌ Error HTTP conectando con Stock API");
                throw;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "❌ Error deserializando JSON de Stock API");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Error inesperado - Usando datos de prueba");
                return GetProductosDePrueba();
            }
        }

        // También actualiza el método GetProductByIdAsync
        /*public async Task<ProductoStock> GetProductByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Obteniendo producto {id} desde Stock API...");

                var token = await GetAccessTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:3000/productos/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error obteniendo producto {id}. Status: {response.StatusCode}");
                    throw new HttpRequestException($"Error obteniendo producto: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();

                // Para producto individual, probablemente devuelva el objeto directo
                return JsonSerializer.Deserialize<ProductoStock>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (JsonException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Stock API no disponible - Buscando producto {id} en datos de prueba");
                var productos = GetProductosDePrueba();
                return productos.FirstOrDefault(p => p.Id == id);
            }
        }*/

        // Agrega esta clase para manejar la respuesta de Stock API
        public class StockApiResponse
        {
            [JsonPropertyName("data")]
            public List<ProductoStock> Data { get; set; } = new List<ProductoStock>();
        }

        public async Task<ProductoStock> GetProductByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"🔍 Obteniendo producto {id} desde Stock API...");

                var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/productos/{id}");
                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("❌ Error obteniendo producto {ProductId}: {StatusCode}", id, response.StatusCode);
                    throw new HttpRequestException($"Stock API returned {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"✅ Producto {id} obtenido de Stock API");

                return JsonSerializer.Deserialize<ProductoStock>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"❌ Stock API no disponible - Buscando producto {id} en datos de prueba");
                var productos = GetProductosDePrueba();
                return productos.FirstOrDefault(p => p.Id == id);
            }
        }

        public async Task<ReservaOutput> CrearReservaAsync(ReservaInput reserva)
        {
            try
            {
                _logger.LogInformation("📝 Creando reserva en Stock API...");

                var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, "/reservas");

                var json = JsonSerializer.Serialize(reserva, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Error creando reserva: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Stock API returned {response.StatusCode}: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("✅ Reserva creada exitosamente en Stock API");

                return JsonSerializer.Deserialize<ReservaOutput>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "❌ Stock API no disponible - Creando reserva de prueba");
                return CrearReservaPrueba(reserva);
            }
        }

        public async Task<ReservaCompleta> ObtenerReservaAsync(int idReserva, int usuarioId)
        {
            try
            {
                _logger.LogInformation($"🔍 Obteniendo reserva {idReserva} desde Stock API...");

                var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/reservas/{idReserva}?usuarioId={usuarioId}");
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("❌ Error obteniendo reserva {ReservaId}: {StatusCode}",
                        idReserva, response.StatusCode);
                    throw new HttpRequestException($"Stock API returned {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"✅ Reserva {idReserva} obtenida de Stock API");

                return JsonSerializer.Deserialize<ReservaCompleta>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"❌ Error obteniendo reserva {idReserva} - Usando datos de prueba");
                return ObtenerReservaPrueba(idReserva, usuarioId);
            }
        }

        public async Task<bool> CancelarReservaAsync(int idReserva, int usuarioId)
        {
            try
            {
                _logger.LogInformation($"🗑️ Cancelando reserva {idReserva} en Stock API...");

                var request = await CreateAuthenticatedRequestAsync(HttpMethod.Delete, $"/reservas/{idReserva}");

                // Agregar el usuarioId en el body según la especificación
                var cancelacionRequest = new { usuarioId, motivo = "Cancelado desde sistema de compras" };
                var json = JsonSerializer.Serialize(cancelacionRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Error cancelando reserva {ReservaId}: {StatusCode} - {Error}",
                        idReserva, response.StatusCode, errorContent);
                    throw new HttpRequestException($"Stock API returned {response.StatusCode}: {errorContent}");
                }

                _logger.LogInformation($"✅ Reserva {idReserva} cancelada exitosamente en Stock API");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error cancelando reserva {idReserva}");
                return false;
            }
        }

        // MÉTODOS DE PRUEBA PARA RESERVAS
        private ReservaOutput CrearReservaPrueba(ReservaInput reserva)
        {
            return new ReservaOutput
            {
                IdReserva = new Random().Next(1000, 9999),
                IdCompra = reserva.IdCompra,
                UsuarioId = reserva.UsuarioId,
                Estado = "confirmado",
                ExpiresAt = DateTime.UtcNow.AddHours(24).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                FechaCreacion = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }

        private ReservaCompleta ObtenerReservaPrueba(int idReserva, int usuarioId)
        {
            return new ReservaCompleta
            {
                IdReserva = idReserva,
                IdCompra = $"COMPRA-{idReserva}",
                UsuarioId = usuarioId,
                Estado = "confirmado",
                ExpiresAt = DateTime.UtcNow.AddHours(24).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                FechaCreacion = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Productos = new List<ProductoReservaDetalle>
                {
                    new ProductoReservaDetalle
                    {
                        IdProducto = 1,
                        Nombre = "Laptop Gaming",
                        Cantidad = 2,
                        PrecioUnitario = 1500.00M
                    }
                }
            };
        }

        // MÉTODO CON DATOS DE PRUEBA
        private List<ProductoStock> GetProductosDePrueba()
        {
            return new List<ProductoStock>
            {
                new ProductoStock
                {
                    Id = 1,
                    Nombre = "Laptop Gaming (PRUEBA)",
                    Descripcion = "Laptop para gaming de alta performance - DATOS DE PRUEBA",
                    Precio = 1500.00M,
                    StockDisponible = 10,
                    PesoKg = 2.5M,
                    Dimensiones = new Dimensiones { LargoCm = 35.0M, AnchoCm = 25.0M, AltoCm = 2.5M },
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
                    Nombre = "Mouse Inalámbrico (PRUEBA)",
                    Descripcion = "Mouse ergonómico inalámbrico - DATOS DE PRUEBA",
                    Precio = 45.50M,
                    StockDisponible = 25,
                    PesoKg = 0.2M,
                    Dimensiones = new Dimensiones { LargoCm = 12.0M, AnchoCm = 6.0M, AltoCm = 3.0M },
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
                }
            };
        }
    }

    public class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_expires_in")]
        public int RefreshExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}