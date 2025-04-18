using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KioskApp.Models;

namespace KioskApp.Services
{
    internal class ProductApiService : IProductApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserApiService _userApiService;

        public ProductApiService(HttpClient httpClient, IUserApiService userApiService)
        {
            _httpClient = httpClient;
            _userApiService = userApiService;
        }

        // Download: fetch raw image stream by URL
        public async Task<Stream> DownloadProductImageAsync(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentNullException(nameof(imageUrl), "Image URL cannot be null or empty.");

            try
            {
                Debug.WriteLine("Downloading product image");
                return await _httpClient.GetStreamAsync(imageUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error downloading image: {ex.Message}");
                throw;
            }
        }

        // GET: api/products/{id}
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            var response = await _userApiService.SendRequestAsync(
                () => new HttpRequestMessage(HttpMethod.Get, $"api/products/{productId}")
            );

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }

        // GET: api/products/getProducts
        public async Task<List<Product>> GetProductsAsync()
        {
            var response = await _userApiService.SendRequestAsync(
                () => new HttpRequestMessage(HttpMethod.Get, "api/products/getProducts")
            );

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Product>>();
        }

        // POST: api/products/addProduct
        public async Task<Product> AddProductAsync(Product product, Stream imageStream, string imageName)
        {
            var content = BuildMultipartContent(product, imageStream, imageName);

            var response = await _userApiService.SendRequestAsync(
                () => new HttpRequestMessage(HttpMethod.Post, "api/products/addProduct") { Content = content }
            );

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }

        // PUT: api/products/updateProduct/{id}
        public async Task<bool> UpdateProductAsync(Product product, Stream imageStream, string imageName)
        {
            var content = BuildMultipartContent(product, imageStream, imageName);

            var response = await _userApiService.SendRequestAsync(
                () => new HttpRequestMessage(HttpMethod.Put, $"api/products/updateProduct/{product.Id}") { Content = content }
            );

            return response.IsSuccessStatusCode;
        }

        // DELETE: api/products/{id}
        public async Task<bool> DeleteProductAsync(int productId)
        {
            var response = await _userApiService.SendRequestAsync(
                () => new HttpRequestMessage(HttpMethod.Delete, $"api/products/{productId}")
            );

            return response.IsSuccessStatusCode;
        }

        // PUT: api/products/toggleVisibility/{id}
        public async Task<bool> ToggleVisibilityAsync(int productId)
        {
            var response = await _userApiService.SendRequestAsync(
                () => new HttpRequestMessage(HttpMethod.Put, $"api/products/toggleVisibility/{productId}")
            );

            return response.IsSuccessStatusCode;
        }

        // POST: api/products/reserve
        public async Task<ProductStockResponse> ReserveProductStockAsync(int productId, int quantity)
        {
            var request = new { ProductId = productId, Quantity = quantity };
            var response = await _userApiService.SendRequestAsync(
                () => new HttpRequestMessage(HttpMethod.Post, "api/products/reserve") { Content = JsonContent.Create(request) }
            );

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductStockResponse>();
        }

        // POST: api/products/release
        public async Task<ProductStockResponse> ReleaseProductStockAsync(int productId, int quantity)
        {
            var request = new { ProductId = productId, Quantity = quantity };
            var response = await _userApiService.SendRequestAsync(
                () => new HttpRequestMessage(HttpMethod.Post, "api/products/release") { Content = JsonContent.Create(request) }
            );

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductStockResponse>();
        }

        // GET: api/products/{id}/availableStock
        public async Task<int> GetAvailableStockAsync(int productId)
        {
            var response = await _userApiService.SendRequestAsync(
                () => new HttpRequestMessage(HttpMethod.Get, $"api/products/{productId}/availableStock")
            );

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        // POST: api/products/{id}/confirmOrder
        public async Task ConfirmOrderAsync(int productId, int quantity)
        {
            var request = new { ProductId = productId, Quantity = quantity };
            var response = await _userApiService.SendRequestAsync(
                () => new HttpRequestMessage(HttpMethod.Post, $"api/products/{productId}/confirmOrder") { Content = JsonContent.Create(request) }
            );

            response.EnsureSuccessStatusCode();
        }

        // Helper: construct multipart form data for product with optional image
        private MultipartFormDataContent BuildMultipartContent(Product product, Stream imageStream, string imageName)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent(product.Id.ToString()), "id" },
                { new StringContent(product.Name ?? string.Empty), "Name" },
                { new StringContent(product.Description ?? string.Empty), "Description" },
                { new StringContent(product.Price?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty), "Price" },
                { new StringContent((product.Stock ?? 0).ToString(CultureInfo.InvariantCulture)), "Stock" },
                { new StringContent(product.ReservedStock.ToString(CultureInfo.InvariantCulture)), "ReservedStock" },
                { new StringContent(product.Category ?? string.Empty), "Category" },
                { new StringContent(product.LastUpdated.ToString("o")), "LastUpdated" }
            };

            if (imageStream != null)
            {
                imageStream.Position = 0;
                var imageContent = new StreamContent(imageStream);
                imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                content.Add(imageContent, "image", imageName);
                product.ImageUrl = $"/images/{imageName}";
            }

            content.Add(new StringContent(product.ImageUrl ?? string.Empty), "ImageUrl");
            return content;
        }
    }
}
