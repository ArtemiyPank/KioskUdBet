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

        //// Подключение к SSE для отслеживания статуса заказа
        //public Task StartMonitoringOrderStatus(int orderId, Action<string> onStatusUpdate, CancellationToken cancellationToken)
        //{
        //    // Выполнение метода на фоновом потоке
        //    return Task.Run(async () =>
        //    {
        //        await StartOrderStatusSseAsync(orderId, status =>
        //        {
        //            // Здесь вы обновляете UI, поэтому убедитесь, что это делается на главном потоке
        //            MainThread.BeginInvokeOnMainThread(() =>
        //            {
        //                // Обновляем UI
        //                onStatusUpdate(status);
        //                Debug.WriteLine($"Order Status: {status}");
        //            });
        //        }, cancellationToken);
        //    });
        //}


        //public async Task StartOrderStatusSseAsync(int orderId, Action<string> onStatusUpdate, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        Debug.WriteLine("Before SendRequestAsync");
        //        var response = await _userApiService.SendRequestAsync(() =>
        //        {
        //            return new HttpRequestMessage(HttpMethod.Get, $"/api/sse/orders/{orderId}");
        //        }, true);

        //        Debug.WriteLine("After GetAsync");
        //        Debug.WriteLine($"response - {response}");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            using (var stream = await response.Content.ReadAsStreamAsync())
        //            {
        //                using (var reader = new StreamReader(stream))
        //                {
        //                    while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        //                    {
        //                        var message = await reader.ReadLineAsync();
        //                        Debug.WriteLine($"Received message: {message}");
        //                        if (!string.IsNullOrEmpty(message))
        //                        {
        //                            // Убираем префикс "data:" из сообщения
        //                            if (message.StartsWith("data:"))
        //                            {
        //                                message = message.Substring("data:".Length).Trim();
        //                            }

        //                            Debug.WriteLine("Before onStatusUpdate(message)");
        //                            onStatusUpdate(message);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            Debug.WriteLine($"Request failed with status code: {response.StatusCode}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"Error in StartOrderStatusSseAsync: {ex.Message}");
        //    }
        //}



        // Подключение к SSE для отслеживания количества всех товаров
        public Task StartMonitoringAllProductsStock(Action<int, int> onQuantityUpdate, CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                await StartAllProductsStockSseAsync((productId, quantity) =>
                {
                    // Обновление UI на главном потоке
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        onQuantityUpdate(productId, quantity);
                        Debug.WriteLine($"Quantity of product {productId}: {quantity}");
                    });
                }, cancellationToken);
            });
        }

        public async Task StartAllProductsStockSseAsync(Action<int, int> onQuantityUpdate, CancellationToken cancellationToken)
        {
            try
            {
                Debug.WriteLine("Before SSE request in all products stock monitor");

                var response = await _userApiService.SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Get, "/api/sse/products/monitor");
                }, true);

                Debug.WriteLine("After SSE request in all products stock monitor");

                if (response.IsSuccessStatusCode)
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                            {
                                var message = await reader.ReadLineAsync();

                                if (!string.IsNullOrEmpty(message))
                                {
                                    // Убираем префикс "data:" из сообщения
                                    if (message.StartsWith("data:"))
                                    {
                                        message = message.Substring("data:".Length).Trim();
                                    }

                                    Debug.WriteLine(message);

                                    // Разбираем сообщение в формате productId:quantity
                                    var parts = message.Split(':');
                                    if (parts.Length == 2 && Int32.TryParse(parts[0], out var productId) && Int32.TryParse(parts[1], out var quantity))
                                    {
                                        onQuantityUpdate(productId, quantity);
                                    }
                                    else
                                    {
                                        Debug.WriteLine($"Invalid message format received: {message}");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"SSE request failed with status code: {response.StatusCode}");
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("SSE monitoring was cancelled.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in SSE monitoring for all products: {ex.Message}");
            }
        }


    }
}
