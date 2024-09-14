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
        private static ConcurrentDictionary<int, List<IObserver<string>>> _productObservers = new ConcurrentDictionary<int, List<IObserver<string>>>();
        private readonly ILogger<SseController> _logger;

        public SseController(ILogger<SseController> logger)
        {
            _logger = logger;
        }

        // SSE для статуса заказов
        [HttpGet("orders/{orderId}")]
        public async Task SseOrderStatus(int orderId)
        {
            // Устанавливаем заголовок SSE
            Response.Headers.Add("Content-Type", "text/event-stream");

            var cancellationToken = HttpContext.RequestAborted;

            // Добавляем наблюдателя для отслеживания изменений статуса заказа
            var observer = new Observer(async status =>
            {
                try
                {
                    // Отправляем данные клиенту
                    await Response.WriteAsync($"data: {status}\n\n");
                    await Response.Body.FlushAsync(); // Отправляем данные немедленно
                }
                catch (IOException)
                {
                    // Клиент отключился или возникла ошибка записи данных
                    Console.WriteLine("Connection to client was lost.");
                }
            });

            // Добавляем наблюдателя в список для данного заказа
            _orderObservers.AddOrUpdate(orderId, new List<IObserver<string>> { observer }, (key, list) =>
            {
                list.Add(observer);
                return list;
            });

            // Ждём завершения запроса, следим за отменой через cancellationToken
            try
            {
                // Ожидание, пока клиент не разорвёт соединение
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Client disconnected.");
            }
            finally
            {
                // Убираем наблюдателя после разрыва соединения
                _orderObservers.TryGetValue(orderId, out var observers);
                observers?.Remove(observer);
            }
        }


        // SSE для контроля количества товаров
        [HttpGet("products/{productId}")]
        public async Task SseProductQuantity(int productId)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");

            var observer = new Observer(async quantity =>
            {
                await Response.WriteAsync($"data: {quantity}\n\n");
                await Response.Body.FlushAsync();
            });

            _productObservers.AddOrUpdate(productId, new List<IObserver<string>> { observer }, (key, list) =>
            {
                list.Add(observer);
                return list;
            });

            // Ожидание закрытия соединения
            await Task.Delay(Timeout.Infinite);
        }

        // Уведомление клиентов об изменении статуса заказа
        public static void NotifyOrderStatusChanged(int orderId, string newStatus)
        {
            Console.WriteLine("In NotifyOrderStatusChanged");
            if (_orderObservers.TryGetValue(orderId, out var observers))
            {
                foreach (var observer in observers)
                {
                    Console.WriteLine("In NotifyOrderStatusChanged foreach");

                    observer.OnNext(newStatus);
                }
            }
        }

        // Уведомление клиентов об изменении количества товара
        public static void NotifyProductQuantityChanged(int productId, string newQuantity)
        {
            if (_productObservers.TryGetValue(productId, out var observers))
            {
                foreach (var observer in observers)
                {
                    observer.OnNext(newQuantity);
                }
            }
        }

        private class Observer : IObserver<string>
        {
            private readonly Func<string, Task> _onNext;

            public Observer(Func<string, Task> onNext)
            {
                _onNext = onNext;
            }

            public void OnNext(string value)
            {
                _onNext(value);
            }

            public void OnError(Exception error) { }

            public void OnCompleted() { }
        }
    }
}
