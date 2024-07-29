using KioskApp.Models;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public interface ICacheService
    {
        Task SaveProductAsync(Product product, Stream imageStream);
        Task<Product> GetProductAsync(int productId);
        string GetProductImagePath(int productId);
        void ClearCache();
        Task DeleteProduct(int productId);
        List<int> GetCachedProductIds();
        Task LogProductCache();
    }
}
