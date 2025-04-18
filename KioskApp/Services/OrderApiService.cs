using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using KioskApp.Helpers;
using KioskApp.Models;

namespace KioskApp.Services
{
    public class OrderApiService : IOrderApiService
    {
        private readonly IUserApiService _userApiService;
        private readonly AppState _appState;

        public OrderApiService(HttpClient httpClient, AppState appState)
        {
            _userApiService = new UserApiService(httpClient);
            _appState = appState;
        }

        // POST: api/order/placeOrder
        public async Task<Order> PlaceOrderAsync(Order order)
        {
            Debug.WriteLine("Placing order via API");
            var response = await _userApiService.SendRequestAsync(() =>
                new HttpRequestMessage(HttpMethod.Post, "api/order/placeOrder")
                {
                    Content = JsonContent.Create(order)
                });

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }

        // GET: api/order
        public async Task<List<Order>> GetOrdersAsync()
        {
            DeserializationHelper.IsDeserializing = true;
            try
            {
                var response = await _userApiService.SendRequestAsync(() =>
                    new HttpRequestMessage(HttpMethod.Get, "api/order"));
                response.EnsureSuccessStatusCode();

                var orders = await response.Content.ReadFromJsonAsync<List<Order>>();
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
            finally
            {
                DeserializationHelper.IsDeserializing = false;
            }
        }

        // GET: api/order/active
        public async Task<List<Order>> GetActiveOrdersAsync()
        {
            DeserializationHelper.IsDeserializing = true;
            try
            {
                var response = await _userApiService.SendRequestAsync(() =>
                    new HttpRequestMessage(HttpMethod.Get, "api/order/active"));
                response.EnsureSuccessStatusCode();

                var activeOrders = await response.Content.ReadFromJsonAsync<List<Order>>();
                if (activeOrders != null)
                {
                    foreach (var order in activeOrders)
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

                return activeOrders;
            }
            finally
            {
                DeserializationHelper.IsDeserializing = false;
            }
        }

        // GET: api/order/user/{userId}/lastOrder
        public async Task<Order> GetLastOrderOrCreateEmptyAsync(int userId)
        {
            DeserializationHelper.IsDeserializing = true;
            try
            {
                var response = await _userApiService.SendRequestAsync(() =>
                {
                    Debug.WriteLine($"Fetching last order for user {userId}");
                    return new HttpRequestMessage(HttpMethod.Get, $"api/order/user/{userId}/lastOrder");
                });

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<Order>();
            }
            finally
            {
                DeserializationHelper.IsDeserializing = false;
            }
        }

        // GET: api/order/{orderId}
        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var response = await _userApiService.SendRequestAsync(() =>
                new HttpRequestMessage(HttpMethod.Get, $"api/order/{orderId}"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }

        // PUT: api/order/{orderId}/status
        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var response = await _userApiService.SendRequestAsync(() =>
                new HttpRequestMessage(HttpMethod.Put, $"api/order/{orderId}/status")
                {
                    Content = JsonContent.Create(status)
                });
            return response.IsSuccessStatusCode;
        }

        // GET: api/order/getOrderStatus/{orderId}
        public async Task<string> GetOrderStatusAsync(int orderId)
        {
            var response = await _userApiService.SendRequestAsync(() =>
                new HttpRequestMessage(HttpMethod.Get, $"api/order/getOrderStatus/{orderId}"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        // PUT: api/order/{orderId}/cancelOrder
        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var response = await _userApiService.SendRequestAsync(() =>
                new HttpRequestMessage(HttpMethod.Put, $"api/order/{orderId}/cancelOrder"));
            return response.IsSuccessStatusCode;
        }

        // PUT: api/order/{orderId}/updateOrder
        public async Task<bool> UpdateOrderAsync(Order order)
        {
            Debug.WriteLine("Updating order via API");
            var response = await _userApiService.SendRequestAsync(() =>
                new HttpRequestMessage(HttpMethod.Put, $"api/order/{order.Id}/updateOrder")
                {
                    Content = JsonContent.Create(order)
                });
            return response.IsSuccessStatusCode;
        }

        // POST: api/order/createEmptyOrder
        public async Task<Order> CreateEmptyOrderAsync(User user)
        {
            var response = await _userApiService.SendRequestAsync(() =>
                new HttpRequestMessage(HttpMethod.Post, "api/order/createEmptyOrder")
                {
                    Content = JsonContent.Create(user)
                });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }
    }

    // Converter to safely deserialize OrderItem without triggering stock operations
    public class SafeOrderItemConverter : JsonConverter<OrderItem>
    {
        public override OrderItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            DeserializationHelper.IsDeserializing = true;
            try
            {
                var item = new OrderItem { ShouldManageStock = false };
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Expected StartObject token");

                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    var prop = reader.GetString();
                    reader.Read();

                    switch (prop)
                    {
                        case "Id":
                        case "id":
                            item.Id = reader.GetInt32();
                            break;
                        case "ProductId":
                        case "productId":
                            item.ProductId = reader.GetInt32();
                            break;
                        case "Quantity":
                        case "quantity":
                            item.InitializeFromJson(item.ProductId, reader.GetInt32());
                            break;
                    }
                }
                return item;
            }
            finally
            {
                DeserializationHelper.IsDeserializing = false;
            }
        }

        public override void Write(Utf8JsonWriter writer, OrderItem value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}
