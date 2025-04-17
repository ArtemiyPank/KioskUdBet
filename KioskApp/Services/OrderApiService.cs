using KioskApp.Helpers;
using KioskApp.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
            try
            {
                // Set global flag to prevent stock operations during deserialization
                DeserializationHelper.IsDeserializing = true;

                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Get, "api/order");
                });

                response.EnsureSuccessStatusCode();

                // Use standard deserialization
                var orders = await response.Content.ReadFromJsonAsync<List<Order>>();

                // Process orders after deserialization
                if (orders != null)
                {
                    foreach (var order in orders)
                    {
                        if (order.OrderItems != null)
                        {
                            foreach (var item in order.OrderItems)
                            {
                                // Disable stock management for display
                                item.ShouldManageStock = false;
                            }
                        }
                    }
                }

                return orders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting all orders: {ex.Message}");
                throw;
            }
            finally
            {
                // Reset global flag
                DeserializationHelper.IsDeserializing = false;
            }
        }


        public async Task<List<Order>> GetActiveOrders()
        {
            try
            {
                // Устанавливаем флаг, чтобы при десериализации не выполнялись операции со складом
                DeserializationHelper.IsDeserializing = true;

                // Отправляем GET-запрос на новый эндпоинт
                var response = await _userApiService.SendRequestAsync(() =>
                    new HttpRequestMessage(HttpMethod.Get, "api/order/active")
                );

                response.EnsureSuccessStatusCode();

                // Стандартная десериализация JSON в List<Order>
                var orders = await response.Content.ReadFromJsonAsync<List<Order>>();

                // После десериализации выключаем управление запасами для каждого item
                if (orders != null)
                {
                    foreach (var order in orders)
                    {
                        if (order.OrderItems != null)
                        {
                            foreach (var item in order.OrderItems)
                            {
                                item.ShouldManageStock = false;
                            }
                        }
                    }
                }

                return orders;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting active orders: {ex.Message}");
                throw;
            }
            finally
            {
                // Сбрасываем флаг
                DeserializationHelper.IsDeserializing = false;
            }
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

    // Add this class to the file
    public class SafeOrderItemConverter : JsonConverter<OrderItem>
    {
        public override OrderItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Set a flag to indicate we're deserializing
            DeserializationHelper.IsDeserializing = true;

            try
            {
                // Create a new OrderItem with ShouldManageStock set to false
                var orderItem = new OrderItem { ShouldManageStock = false };

                // Read the JSON object
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected start of object");
                }

                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException("Expected property name");
                    }

                    var propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "id":
                        case "Id":
                            orderItem.Id = reader.GetInt32();
                            break;
                        case "productId":
                        case "ProductId":
                            orderItem.ProductId = reader.GetInt32();
                            break;
                        case "quantity":
                        case "Quantity":
                            // Set quantity directly to avoid triggering stock operations
                            orderItem.Quantity = reader.GetInt32();
                            break;
                            // Add other properties as needed
                    }
                }

                return orderItem;
            }
            finally
            {
                // Reset the flag
                DeserializationHelper.IsDeserializing = false;
            }
        }

        public override void Write(Utf8JsonWriter writer, OrderItem value, JsonSerializerOptions options)
        {
            // Default serialization
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
