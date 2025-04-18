using KioskAPI.Models;

namespace KioskAPI.Services
{
    public interface IProductService
    {
        // GET: retrieve all products
        Task<List<Product>> GetProductsAsync();

        // POST: add a new product
        Task<Product> AddProductAsync(Product product);

        // GET: retrieve a product by its ID
        Task<Product?> GetProductByIdAsync(int productId);

        // PUT: update an existing product
        Task<Product> UpdateProductAsync(int id, Product product);

        // DELETE: remove a product by its ID
        Task<bool> DeleteProductAsync(int productId);

        // PUT: toggle the visibility flag of a product
        Task<bool> ToggleVisibilityAsync(int id);

        // POST: reserve stock for a product
        Task<int> ReserveProductStockAsync(int productId, int quantity);

        // POST: release reserved stock for a product
        Task<int> ReleaseProductStockAsync(int productId, int quantity);

        // GET: get the current stock level of a product
        Task<int> GetStockAsync(int productId);

        // GET: get the current reserved stock level of a product
        Task<int> GetReservedStockAsync(int productId);

        // POST: confirm an order by deducting delivered quantity from stock
        Task ConfirmOrderAsync(int productId, int quantity);
    }
}
