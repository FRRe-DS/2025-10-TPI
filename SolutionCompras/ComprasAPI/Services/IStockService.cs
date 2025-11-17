// Services/IStockService.cs
using ComprasAPI.Models.DTOs;

namespace ComprasAPI.Services
{
    public interface IStockService
    {
        Task<List<ProductoStock>> GetAllProductsAsync();
        Task<ProductoStock> GetProductByIdAsync(int id);

        // ✅ AGREGAR ESTOS MÉTODOS NUEVOS
        Task<ReservaOutput> CrearReservaAsync(ReservaInput reserva);
        Task<ReservaCompleta> ObtenerReservaAsync(int idReserva, int usuarioId);

        Task<bool> CancelarReservaAsync(int idReserva, int usuarioId);
    }
}