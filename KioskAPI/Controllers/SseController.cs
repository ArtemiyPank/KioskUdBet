using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SseController : ControllerBase
    {
        // Словари для наблюдателей (наблюдатели для статусов заказов и количества товаров)
        private static ConcurrentDictionary<string, List<IObserver<(int, int, int)>>> _productObservers = new ConcurrentDictionary<string, List<IObserver<(int, int, int)>>>();
        private readonly ILogger<SseController> _logger;

        public SseController(ILogger<SseController> logger)
        {
            _logger = logger;
        }

        // SSE для контроля количества всех товаров
        [HttpGet("products/monitor")]
        public async Task SseProductQuantities()
        {
            Console.WriteLine("In SseProductQuantities");
            Response.Headers.Add("Content-Type", "text/event-stream");

            var cancellationToken = HttpContext.RequestAborted;
            var observerId = Guid.NewGuid().ToString();

            var observer = new ObserverProducts(async (productId, stock, reservedQuantity) =>
            {
                // Отправляем обновления для каждого продукта
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Response.WriteAsync($"data: {productId}:{stock}:{reservedQuantity}\n\n");
                    await Response.Body.FlushAsync();
                }
            });

            // Добавляем наблюдателя для всех товаров с уникальным ID
            _productObservers.AddOrUpdate(observerId, new List<IObserver<(int, int, int)>> { observer }, (key, list) =>
            {
                list.Add(observer);
                return list;
            });

            try
            {
                // Ожидание закрытия соединения
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"SSE connection closed by client. Observer ID: {observerId}");
            }
            finally
            {
                // Удаление наблюдателя при закрытии соединения
                if (_productObservers.TryGetValue(observerId, out var observers))
                {
                    lock (observers)  // Потокобезопасное удаление
                    {
                        observers.Remove(observer);
                    }

                    if (observers.Count == 0)
                    {
                        _productObservers.TryRemove(observerId, out _);
                    }
                }
            }
        }

        // Уведомление клиентов об изменении количества товара
        public static void NotifyProductQuantityChanged(int productId, int stock, int reservedQuantity)
        {
            Console.WriteLine("In NotifyProductQuantityChanged");
            foreach (var observersList in _productObservers.Values)
            {
                lock (observersList)  // Потокобезопасное уведомление
                {
                    foreach (var observer in observersList)
                    {
                        Console.WriteLine("In NotifyProductQuantityChanged foreach");

                        observer.OnNext((productId, stock, reservedQuantity));
                    }
                }
            }
        }

        // Observer для отслеживания количества товаров
        private class ObserverProducts : IObserver<(int, int, int)>
        {
            private readonly Func<int, int, int, Task> _onNext;

            public ObserverProducts(Func<int, int, int, Task> onNext)
            {
                _onNext = onNext;
            }

            public void OnNext((int, int, int) value)
            {
                Console.WriteLine("In OnNext");

                _onNext(value.Item1, value.Item2, value.Item3);
            }

            public void OnError(Exception error) { }

            public void OnCompleted() { }
        }
    }
}
