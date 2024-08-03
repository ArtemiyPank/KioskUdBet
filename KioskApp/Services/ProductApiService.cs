using KioskApp.Models;
using KioskApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    internal class ProductApiService : IProductApiService
    {
        private readonly HttpClient _httpClient;
        private IUserApiService _userApiService;


        public ProductApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _userApiService = DependencyService.Get<IUserApiService>();
        }


        public async Task<Stream> DownloadProductImage(string imageUrl)
        {
            return await _httpClient.GetStreamAsync(imageUrl);
        }

        public async Task<Product> GetProductById(int productId)
        {

            var response = await _userApiService.SendRequestAsync(() => {
                return new HttpRequestMessage(HttpMethod.Post, $"api/products/{productId}");
            });

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }


        public async Task<List<Product>> GetProducts()
        {
            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Get, "api/products/getProducts");
            });

            return await response.Content.ReadFromJsonAsync<List<Product>>();
        }

        public async Task<Product> AddProduct(Product product, Stream imageStream, string imageName)
        {
            try
            {
                var content = new MultipartFormDataContent
                {
                    { new StringContent(product.Name ?? string.Empty), "Name" },
                    { new StringContent(product.Description ?? string.Empty), "Description" },
                    { new StringContent(product.Price?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty), "Price" }, // Форматирование Price
                    { new StringContent(product.Stock?.ToString(CultureInfo.InvariantCulture) ?? string.Empty), "Stock" },
                    { new StringContent(product.Category ?? string.Empty), "Category" },
                    { new StringContent(product.LastUpdated.ToString("o")), "LastUpdated" } // Преобразование даты в строку в формате ISO 8601
                };

                if (imageStream != null)
                {
                    Debug.WriteLine($"Image stream length: {imageStream.Length}");
                    imageStream.Position = 0; // Обнуление позиции потока перед использованием
                    var imageContent = new StreamContent(imageStream);
                    imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                    content.Add(imageContent, "image", imageName);

                    // Установили ImageUrl только если изображение было загружено
                    product.ImageUrl = $"/images/{imageName}";
                }

                // Добавили ImageUrl в запрос
                content.Add(new StringContent(product.ImageUrl ?? string.Empty), "ImageUrl");

                Debug.WriteLine("Sending the following content:");
                foreach (var item in content)
                {
                    if (item is StringContent stringContent)
                    {
                        Debug.WriteLine($"StringContent: {await stringContent.ReadAsStringAsync()}");
                    }
                    else if (item is StreamContent streamContent)
                    {
                        Debug.WriteLine($"StreamContent: {imageName}");
                    }
                }


                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Post, "api/products/addProduct")
                    {
                        Content = content
                    };
                });

                Debug.WriteLine($"Response status code: {response.StatusCode}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response body: {responseBody}");
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
                    { new StringContent(product.Price?.ToString("0.00", CultureInfo.InvariantCulture) ?? string.Empty), "Price" }, // Форматирование Price
                    { new StringContent(product.Stock?.ToString(CultureInfo.InvariantCulture) ?? string.Empty), "Stock" },
                    { new StringContent(product.Category ?? string.Empty), "Category" },
                    { new StringContent(product.LastUpdated.ToString("o")), "LastUpdated" }
                };

                if (imageStream != null)
                {
                    Debug.WriteLine($"Image stream length: {imageStream.Length}");
                    imageStream.Position = 0; // Обнуление позиции потока перед использованием
                    var imageContent = new StreamContent(imageStream);
                    imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                    content.Add(imageContent, "image", imageName);

                    product.ImageUrl = $"/images/{imageName}";
                }
                else
                {
                    Debug.WriteLine("No image provided for the product.");
                }

                content.Add(new StringContent(product.ImageUrl ?? string.Empty), "ImageUrl");

                // Логирование содержимого перед отправкой
                Debug.WriteLine("Sending the following content:");
                foreach (var item in content)
                {
                    if (item is StringContent stringContent)
                    {
                        Debug.WriteLine($"StringContent: {await stringContent.ReadAsStringAsync()}");
                    }
                    else if (item is StreamContent streamContent)
                    {
                        Debug.WriteLine($"StreamContent: {imageName}");
                    }
                }

                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Put, $"api/products/updateProduct/{product.Id}") { Content = content };
                });

                // Логирование ответа сервера
                Debug.WriteLine($"Response status code: {response.StatusCode}");
                var responseBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response body: {responseBody}");
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
            Debug.WriteLine($"productId - {productId}");

            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Delete, $"api/products/{productId}");
            });

            return response.IsSuccessStatusCode;
        }


        public async Task<bool> ToggleVisibility(int productId)
        {
            Debug.WriteLine("I am in ToggleVisibility in Api Service");

            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Put, $"api/products/toggleVisibility/{productId}");
            });

            return response.IsSuccessStatusCode;
        }




        // Place an order on the server
        public async Task<Order> PlaceOrder(Order order)
        {
            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Post, "/api/orders")
                {
                    Content = JsonContent.Create(order)
                };
            });

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Order>();
        }
    }
}
