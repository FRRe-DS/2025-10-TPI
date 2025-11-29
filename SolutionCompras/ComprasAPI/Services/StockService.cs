using ComprasAPI.Models.DTOs;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ComprasAPI.Services
{
    public class StockService : IStockService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StockService> _logger;

        private string _cachedToken;
        private DateTime _tokenExpiry;

        public StockService(HttpClient httpClient, ILogger<StockService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public Task<bool> CancelarReservaAsync(int idReserva, int usuarioId)
        {
            throw new NotImplementedException();
        }

        public Task<ReservaOutput> CrearReservaAsync(ReservaInput reserva)
        {
            throw new NotImplementedException();
        }

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
                _logger.LogError(ex, "💥 Error inesperado obteniendo productos");
                throw;
            }
        }

        public async Task<ProductoStock> GetProductByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"🔍 Obteniendo producto {id} desde Stock API...");

                var token = await GetAccessTokenAsync();
                var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:3000/productos/{id}");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"❌ Producto {id} no encontrado en Stock API");
                    return null;
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ Error obteniendo producto {id}. Status: {response.StatusCode} - {errorContent}");
                    throw new HttpRequestException($"Error obteniendo producto: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"✅ Producto {id} obtenido de Stock API");

                return JsonSerializer.Deserialize<ProductoStock>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, $"❌ Error HTTP obteniendo producto {id} de Stock API");
                throw;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, $"❌ Error deserializando producto {id} de Stock API");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"💥 Error inesperado obteniendo producto {id}");
                throw;
            }
        }

        public Task<ReservaCompleta> ObtenerReservaAsync(int idReserva, int usuarioId)
        {
            throw new NotImplementedException();
        }

        // MÉTODO PARA OBTENER TOKEN DE KEYCLOAK
        private async Task<string> GetAccessTokenAsync()
        {
            // Verificar si el token está en caché y es válido
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _cachedToken;
            }

            try
            {
                _logger.LogInformation("Obteniendo token de Keycloak...");

                var tokenEndpoint = "https://keycloak.cubells.com.ar/realms/ds-2025-realm/protocol/openid-connect/token";
                var clientId = "grupo-08";
                var clientSecret = "248f42b5-7007-47d1-a94e-e8941f352f6f";

                var tokenRequest = new List<KeyValuePair<string, string>>
                {
                    new("client_id", clientId),
                    new("client_secret", clientSecret),
                    new("grant_type", "client_credentials")
                };

                var content = new FormUrlEncodedContent(tokenRequest);

                // Usar una instancia temporal de HttpClient para evitar conflictos
                using var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(tokenEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error obteniendo token de Keycloak: {StatusCode} - {Error}",
                        response.StatusCode, errorContent);
                    throw new Exception($"Error obteniendo token: {response.StatusCode}");
                }

                var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>();
                _cachedToken = tokenResponse.AccessToken;
                _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // Restar 60 segundos de margen

                _logger.LogInformation("Token de Keycloak obtenido exitosamente");
                return _cachedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico obteniendo token de Keycloak");
                throw;
            }
        }

        // Agrega esta clase para manejar la respuesta de Stock API
        public class StockApiResponse
        {
            [JsonPropertyName("data")]
            public List<ProductoStock> Data { get; set; } = new List<ProductoStock>();
        }
    }

    // Model para la respuesta del token
    public class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
}