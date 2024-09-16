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
        private static ConcurrentDictionary<int, List<IObserver<string>>> _orderObservers = new ConcurrentDictionary<int, List<IObserver<string>>>();
        private static ConcurrentDictionary<string, List<IObserver<(int, int)>>> _productObservers = new ConcurrentDictionary<string, List<IObserver<(int, int)>>>();
        private readonly ILogger<SseController> _logger;

        public SseController(ILogger<SseController> logger)
        {
            _logger = logger;
        }

        //// SSE для статуса заказов
        //[HttpGet("orders/{orderId}")]
        //public async Task SseOrderStatus(int orderId)
        //{
        //    // Устанавливаем заголовок SSE
        //    Response.Headers.Add("Content-Type", "text/event-stream");

        //    var cancellationToken = HttpContext.RequestAborted;

        //    // Добавляем наблюдателя для отслеживания изменений статуса заказа
        //    var observer = new Observer(async status =>
        //    {
        //        try
        //        {
        //            // Отправляем данные клиенту
        //            await Response.WriteAsync($"data: {status}\n\n");
        //            await Response.Body.FlushAsync(); // Отправляем данные немедленно
        //        }
        //        catch (IOException)
        //        {
        //            // Клиент отключился или возникла ошибка записи данных
        //            Console.WriteLine("Connection to client was lost.");
        //        }
        //    });

        //    // Добавляем наблюдателя в список для данного заказа
        //    _orderObservers.AddOrUpdate(orderId, new List<IObserver<string>> { observer }, (key, list) =>
        //    {
        //        list.Add(observer);
        //        return list;
        //    });

        //    // Ждём завершения запроса, следим за отменой через cancellationToken
        //    try
        //    {
        //        // Ожидание, пока клиент не разорвёт соединение
        //        await Task.Delay(Timeout.Infinite, cancellationToken);
        //    }
        //    catch (TaskCanceledException)
        //    {
        //        Console.WriteLine("Client disconnected.");
        //    }
        //    finally
        //    {
        //        // Убираем наблюдателя после разрыва соединения
        //        _orderObservers.TryGetValue(orderId, out var observers);
        //        observers?.Remove(observer);
        //    }
        //}

        //// Уведомление клиентов об изменении статуса заказа
        //public static void NotifyOrderStatusChanged(int orderId, string newStatus)
        //{
        //    Console.WriteLine("In NotifyOrderStatusChanged");
        //    if (_orderObservers.TryGetValue(orderId, out var observers))
        //    {
        //        foreach (var observer in observers)
        //        {
        //            Console.WriteLine("In NotifyOrderStatusChanged foreach");

        //            observer.OnNext(newStatus);
        //        }
        //    }
        //}

        // SSE для контроля количества всех товаров
        [HttpGet("products/monitor")]
        public async Task SseProductQuantities()
        {
            Console.WriteLine("In SseProductQuantities");
            Response.Headers.Add("Content-Type", "text/event-stream");

            var cancellationToken = HttpContext.RequestAborted;
            var observerId = Guid.NewGuid().ToString();

            var observer = new ObserverProducts(async (productId, quantity) =>
            {
                // Отправляем обновления для каждого продукта
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Response.WriteAsync($"data: {productId}:{quantity}\n\n");
                    await Response.Body.FlushAsync();
                }
            });

            // Добавляем наблюдателя для всех товаров с уникальным ID
            _productObservers.AddOrUpdate(observerId, new List<IObserver<(int, int)>> { observer }, (key, list) =>
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
        public static void NotifyProductQuantityChanged(int productId, int newQuantity)
        {
            Console.WriteLine("In NotifyProductQuantityChanged");
            foreach (var observersList in _productObservers.Values)
            {
                lock (observersList)  // Потокобезопасное уведомление
                {
                    foreach (var observer in observersList)
                    {
                        Console.WriteLine("In NotifyProductQuantityChanged foreach");

                        observer.OnNext((productId, newQuantity));
                    }
                }
            }
        }

        // Observer для отслеживания количества товаров
        private class ObserverProducts : IObserver<(int, int)>
        {
            private readonly Func<int, int, Task> _onNext;

            public ObserverProducts(Func<int, int, Task> onNext)
            {
                _onNext = onNext;
            }

            public void OnNext((int, int) value)
            {
                Console.WriteLine("In OnNext");

                _onNext(value.Item1, value.Item2);
            }

            public void OnError(Exception error) { }

            public void OnCompleted() { }
        }

        //// Observer для отслеживания статуса заказа
        //private class Observer : IObserver<string>
        //{
        //    private readonly Func<string, Task> _onNext;

        //    public Observer(Func<string, Task> onNext)
        //    {
        //        _onNext = onNext;
        //    }

        //    public void OnNext(string value)
        //    {
        //        _onNext(value);
        //    }

        //    public void OnError(Exception error) { }

        //    public void OnCompleted() { }
        //}
    }
}
