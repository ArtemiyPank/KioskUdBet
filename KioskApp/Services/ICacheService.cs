using KioskApp.Models;

namespace KioskApp.Services
{
    public interface ICacheService
    {
        Task SaveProductAsync(Product product, Stream imageStream);
        Task<Product> GetProductAsync(int productId);
        Task<string> GetProductImagePath(int productId);
        void ClearCache();
        Task DeleteProduct(int productId);
        List<int> GetCachedProductIds();
        Task LogProductCache();
        void PrintCacheDirectoryStructure(string? directoryPath = null, string indent = "");
    }
}
