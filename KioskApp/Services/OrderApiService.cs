using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using KioskApp.Models;
using KioskApp.Helpers;
using System.Diagnostics;
using System.Text.Json;

namespace KioskApp.Services
{
    public class OrderApiService : IOrderApiService
    {
        private readonly UserApiService _userApiService;
        private readonly AppState _appState;

        public OrderApiService(HttpClient httpClient, AppState appState)
        {
            _userApiService = new UserApiService(httpClient);
            _appState = appState;
        }

        public async Task<Order> PlaceOrder(Order order)
        {
            Debug.WriteLine("In PlaceOrder in OrderApiService");
            string json = JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true });
            Debug.WriteLine(json);

            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Post, "api/order/placeOrder")
                {
                    Content = JsonContent.Create(order)
                };
            });

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }

        public async Task<List<Order>> GetOrders()
        {
            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Get, "api/order");
            });

            Debug.WriteLine(response.ToString());

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<Order>>();
        }


        public async Task<Order> GetLastOrderOrCreateEmpty(int userId)
        {
            try
            {
                // Установить флаг десериализации
                DeserializationHelper.IsDeserializing = true;

                var response = await _userApiService.SendRequestAsync(() =>
                {
                    Debug.WriteLine($"Sending GET request to /api/order/user/{userId}/lastOrder");
                    return new HttpRequestMessage(HttpMethod.Get, $"/api/order/user/{userId}/lastOrder");
                });

                Debug.WriteLine($"Response Status Code: {response.StatusCode}");
                Debug.WriteLine($"Response Content: {await response.Content.ReadAsStringAsync()}");

                var order = await response.Content.ReadFromJsonAsync<Order>();

                return order;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in GetLastOrderOrCreateEmpty: {ex.Message}");
                throw;
            }
            finally
            {
                // Сбросить флаг десериализации
                DeserializationHelper.IsDeserializing = false;
            }
        }




        public async Task<Order> GetOrderById(int orderId)
        {
            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Get, $"api/order/{orderId}");
            });

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }

        public async Task<bool> UpdateOrderStatus(int orderId, string status)
        {
            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Put, $"api/order/{orderId}/status")
                {
                    Content = JsonContent.Create(status)
                };
            });

            return response.IsSuccessStatusCode;
        }

        public async Task<string> GetOrderStatus(int orderId)
        {
            try
            {
                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Get, $"api/order/getOrderStatus/{orderId}");
                });

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception("Failed to fetch order status from the server.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching order status: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> CancelOrder(int orderId)
        {
            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Put, $"api/order/{orderId}/cancelOrder");
            });

            return response.IsSuccessStatusCode;
        }

        // Новый метод для обновления заказа
        public async Task<bool> UpdateOrder(Order order)
        {
            Debug.WriteLine("In UpdateOrder");
            //Debug.WriteLine(JsonSerializer.Serialize(order, new JsonSerializerOptions { WriteIndented = true }));
            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Put, $"api/order/{order.Id}/updateOrder")
                {
                    Content = JsonContent.Create(order)
                };
            });

            return response.IsSuccessStatusCode;
        }

        // Метод для создания нового пустого заказа после доставки
        public async Task<Order> CreateEmptyOrder(User user)
        {
            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Post, "api/order/createEmptyOrder")
                {
                    Content = JsonContent.Create(user)
                };
            });

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }
    }
}
