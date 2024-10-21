using KioskApp.Models;
using KioskApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    internal class ProductApiService : IProductApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IUserApiService _userApiService;




        public ProductApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _userApiService = DependencyService.Get<IUserApiService>();
        }

        public async Task<Stream> DownloadProductImage(string imageUrl)
        {
            try
            {
                Debug.WriteLine("in DownloadProductImage");

                if (string.IsNullOrEmpty(imageUrl))
                {
                    Debug.WriteLine("Error: imageUrl is null or empty.");
                    throw new ArgumentNullException(nameof(imageUrl), "Image URL cannot be null or empty.");
                }

                return await _httpClient.GetStreamAsync(imageUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error downloading product image: {ex.Message}");
                throw;
            }
        }

        public async Task<Product> GetProductById(int productId)
        {
            try
            {
                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Get, $"api/products/{productId}");
                });

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Product>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting product by ID: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Product>> GetProducts()
        {
            try
            {
                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Get, "api/products/getProducts");
                });

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Product>>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting products: {ex.Message}");
                throw;
            }
        }

        public async Task<Product> AddProduct(Product product, Stream imageStream, string imageName)
        {
            try
            {
                var content = new MultipartFormDataContent
                {
                    { new StringContent(product.Name ?? string.Empty), "Name" },
                    { new StringContent(product.Description ?? string.Empty), "Description" },
                    { new StringContent(product.Price?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty), "Price" },
                    { new StringContent(product.Stock?.ToString(CultureInfo.InvariantCulture) ?? string.Empty), "Stock" },
                    { new StringContent(product.ReservedStock.ToString(CultureInfo.InvariantCulture) ?? string.Empty), "ReservedStock" },
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

                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Post, "api/products/addProduct")
                    {
                        Content = content
                    };
                });

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Product>();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HttpRequestException: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while adding product: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateProduct(Product product, Stream imageStream, string imageName)
        {
            try
            {
                var content = new MultipartFormDataContent
                {
                    { new StringContent(product.Id.ToString()), "Id" },
                    { new StringContent(product.Name ?? string.Empty), "Name" },
                    { new StringContent(product.Description ?? string.Empty), "Description" },
                    { new StringContent(product.Price?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty), "Price" },
                    { new StringContent(product.Stock?.ToString(CultureInfo.InvariantCulture) ?? string.Empty), "Stock" },
                    { new StringContent(product.ReservedStock.ToString(CultureInfo.InvariantCulture) ?? string.Empty), "ReservedStock" },
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

                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Put, $"api/products/updateProduct/{product.Id}")
                    {
                        Content = content
                    };
                });

                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HttpRequestException: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error while updating product: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            try
            {
                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Delete, $"api/products/{productId}");
                });

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting product: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> ToggleVisibility(int productId)
        {
            try
            {
                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Put, $"api/products/toggleVisibility/{productId}");
                });

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error toggling visibility: {ex.Message}");
                throw;
            }
        }

        public async Task<Order> PlaceOrder(Order order)
        {
            try
            {
                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Post, "api/orders")
                    {
                        Content = JsonContent.Create(order)
                    };
                });

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Order>();
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"HttpRequestException: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error placing order: {ex.Message}");
                throw;
            }
        }

        // Метод для резервирования товара
        public async Task<ProductStockResponse> ReserveProductStock(int productId, int quantity)
        {
            var requestBody = new
            {
                ProductId = productId,
                Quantity = quantity
            };

            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Post, "api/products/reserve")
                {
                    Content = JsonContent.Create(requestBody)
                };
            });

            response.EnsureSuccessStatusCode();

            // Десериализуем объект, содержащий доступные и зарезервированные запасы
            return await response.Content.ReadFromJsonAsync<ProductStockResponse>();
        }

        // Метод для освобождения товара
        public async Task<ProductStockResponse> ReleaseProductStock(int productId, int quantity)
        {
            var requestBody = new
            {
                ProductId = productId,
                Quantity = quantity
            };

            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Post, "api/products/release")
                {
                    Content = JsonContent.Create(requestBody)
                };
            });

            response.EnsureSuccessStatusCode();

            // Десериализуем объект, содержащий доступные и зарезервированные запасы
            return await response.Content.ReadFromJsonAsync<ProductStockResponse>();
        }

        // Получение доступного количества товара
        public async Task<int> GetAvailableStock(int productId)
        {
            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Get, $"api/products/{productId}/availableStock");
            });

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<int>();
        }

        // Новый метод для подтверждения заказа
        public async Task ConfirmOrder(int productId, int quantity)
        {
            var requestBody = new
            {
                ProductId = productId,
                Quantity = quantity
            };

            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Post, $"api/products/{productId}/confirmOrder")
                {
                    Content = JsonContent.Create(requestBody)
                };
            });

            response.EnsureSuccessStatusCode();
        }
    }
}
