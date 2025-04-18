using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace KioskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SseController : ControllerBase
    {
        private static readonly ConcurrentDictionary<
            string,
            List<IObserver<(int productId, int stock, int reserved)>>>
            _productObservers = new();

        private readonly ILogger<SseController> _logger;

        public SseController(ILogger<SseController> logger)
        {
            _logger = logger;
        }

        // GET: api/sse/products/monitor
        [HttpGet("products/monitor")]
        public async Task SseProductQuantities()
        {
            _logger.LogInformation("Client connected to SSE product monitor");
            Response.Headers.Add("Content-Type", "text/event-stream");

            var cancellationToken = HttpContext.RequestAborted;
            var observerId = Guid.NewGuid().ToString();

            // Observer that pushes product quantity updates to the response stream
            var observer = new ProductQuantityObserver(async (productId, stock, reserved) =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await Response.WriteAsync($"data: {productId}:{stock}:{reserved}\n\n");
                    await Response.Body.FlushAsync();
                }
            });

            // Register the observer
            _productObservers.AddOrUpdate(
                observerId,
                _ => new List<IObserver<(int, int, int)>> { observer },
                (_, list) =>
                {
                    list.Add(observer);
                    return list;
                });

            try
            {
                // Keep connection open until client disconnects
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("SSE client disconnected: {ObserverId}", observerId);
            }
            finally
            {
                // Unregister observer on disconnect
                if (_productObservers.TryGetValue(observerId, out var observers))
                {
                    lock (observers)
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

        // Called by services to broadcast quantity updates to all observers
        public static void NotifyProductQuantityChanged(int productId, int stock, int reserved)
        {
            foreach (var observers in _productObservers.Values)
            {
                lock (observers)
                {
                    foreach (var observer in observers)
                    {
                        observer.OnNext((productId, stock, reserved));
                    }
                }
            }
        }

        // Internal observer for product quantity SSE
        private class ProductQuantityObserver : IObserver<(int productId, int stock, int reserved)>
        {
            private readonly Func<int, int, int, Task> _onNext;

            public ProductQuantityObserver(Func<int, int, int, Task> onNext)
            {
                _onNext = onNext;
            }

            public void OnNext((int productId, int stock, int reserved) value)
            {
                _ = _onNext(value.productId, value.stock, value.reserved);
            }

            public void OnError(Exception error) { /* No action needed */ }

            public void OnCompleted() { /* No action needed */ }
        }
    }
}
