using KioskApp.Models;
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


        public ProductApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }



        public async Task<List<Product>> GetProducts()
        {
            Debug.WriteLine($"СССССССССССУУУУУУУУУУУУУККККККККККККККККААААААААААААААААААА");
            var a = await _httpClient.GetFromJsonAsync<List<Product>>("api/products/getProducts");
            Debug.WriteLine($"БЛЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯЯ");
            return a;
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

                    // Установим ImageUrl только если изображение было загружено
                    product.ImageUrl = $"/images/{imageName}";
                }

                // Добавим ImageUrl в запрос
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

                var response = await _httpClient.PostAsync("api/products/addProduct", content);


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


        public async Task<Stream> DownloadProductImage(string imageUrl)
        {
            return await _httpClient.GetStreamAsync(imageUrl);
        }

        public async Task<Product> GetProductById(int productId)
        {
            var response = await _httpClient.GetAsync($"api/products/{productId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Product>();
        }


        public async Task<bool> HideProduct(int productId)
        {
            var response = await _httpClient.PutAsync($"api/products/hide/{productId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            Debug.WriteLine($"productId - {productId}");
            var response = await _httpClient.DeleteAsync($"api/products/{productId}");
            return response.IsSuccessStatusCode;
        }


        // Place an order on the server
        public async Task<Order> PlaceOrder(Order order)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/orders", order);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
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

                var url = $"api/products/updateProduct/{product.Id}";
                Debug.WriteLine($"Sending PUT request to URL: {url}");
                var response = await _httpClient.PutAsync(url, content);

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
    }
}
