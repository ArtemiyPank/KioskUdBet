using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public class SseService : ISseService
    {
        private readonly IUserApiService _userApiService;
        private readonly HttpClient _httpClient;

        public SseService(IUserApiService userApiService, HttpClient httpClient)
        {
            _userApiService = userApiService;
            _httpClient = httpClient;
        }

        // Подключение к SSE для отслеживания статуса заказа
        public Task StartMonitoringOrderStatus(int orderId, Action<string> onStatusUpdate, CancellationToken cancellationToken)
        {
            // Выполнение метода на фоновом потоке
            return Task.Run(async () =>
            {
                await StartOrderStatusSseAsync(orderId, status =>
                {
                    // Здесь вы обновляете UI, поэтому убедитесь, что это делается на главном потоке
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Обновляем UI
                        onStatusUpdate(status);
                        Debug.WriteLine($"Order Status: {status}");
                    });
                }, cancellationToken);
            });
        }


        public async Task StartOrderStatusSseAsync(int orderId, Action<string> onStatusUpdate, CancellationToken cancellationToken)
        {
            try
            {
                Debug.WriteLine("Before SendRequestAsync");
                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Get, $"/api/sse/orders/{orderId}");
                }, true);

                Debug.WriteLine("After GetAsync");
                Debug.WriteLine($"response - {response}");

                if (response.IsSuccessStatusCode)
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                            {
                                var message = await reader.ReadLineAsync();
                                Debug.WriteLine($"Received message: {message}");
                                if (!string.IsNullOrEmpty(message))
                                {
                                    // Убираем префикс "data:" из сообщения
                                    if (message.StartsWith("data:"))
                                    {
                                        message = message.Substring("data:".Length).Trim();
                                    }

                                    onStatusUpdate(message);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Request failed with status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in StartOrderStatusSseAsync: {ex.Message}");
            }
        }



        // Подключение к SSE для отслеживания количества товара
        public Task StartMonitoringOrderStatusProductQuantity(int productId, Action<string> onQuantityUpdate)
        {
            // Выполнение метода на фоновом потоке
            return Task.Run(async () =>
            {
                await StartProductQuantitySseAsync(productId, quantity =>
                {
                    // Здесь вы обновляете UI, поэтому убедитесь, что это делается на главном потоке
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        // Обновляем UI
                        onQuantityUpdate(quantity);
                        Debug.WriteLine($"Quantity of product {productId}: {quantity}");
                    });
                });
            });
        }


        public async Task StartProductQuantitySseAsync(int productId, Action<string> onQuantityUpdate)
        {
            var response = await _userApiService.SendRequestAsync(() =>
            {
                return new HttpRequestMessage(HttpMethod.Get, $"/api/sse/products/{productId}");
            });

            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            var message = await reader.ReadLineAsync();
                            if (!string.IsNullOrEmpty(message))
                            {
                                // Убираем префикс "data:" из сообщения
                                    if (message.StartsWith("data:"))
                                {
                                    message = message.Substring("data:".Length).Trim();
                                }
                                onQuantityUpdate(message);
                            }
                        }
                    }
                }
            }
        }
    }
}
