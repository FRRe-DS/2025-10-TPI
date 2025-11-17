using ComprasAPI.Models.DTOs;

namespace ComprasAPI.Services
{
    public interface IStockService
    {
        Task<List<ProductoStock>> GetAllProductsAsync();
        Task<ProductoStock> GetProductByIdAsync(int id);
    }
}