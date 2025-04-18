using System.Diagnostics;

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

        // Starts monitoring stock updates for all products via SSE
        public Task StartMonitoringAllProductsStockAsync(
            Action<int, int, int> onQuantityUpdate,
            CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                await StartAllProductsStockSseAsync((productId, stock, reserved) =>
                {
                    // Update UI on the main thread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        onQuantityUpdate(productId, stock, reserved);
                    });
                }, cancellationToken);
            });
        }

        // Connects to the SSE endpoint and parses stock update events
        public async Task StartAllProductsStockSseAsync(
            Action<int, int, int> onQuantityUpdate,
            CancellationToken cancellationToken)
        {
            try
            {
                Debug.WriteLine("Initiating SSE connection for product stock");

                var response = await _userApiService.SendRequestAsync(
                    () => new HttpRequestMessage(HttpMethod.Get, "/api/sse/products/monitor"),
                    isSseRequest: true);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"SSE endpoint returned {response.StatusCode}");
                    return;
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Remove "data:" prefix if present
                    if (line.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                        line = line.Substring(5).Trim();

                    Debug.WriteLine($"SSE data received: {line}");
                    var parts = line.Split(':');
                    if (parts.Length != 3) continue;

                    if (int.TryParse(parts[0], out var id) &&
                        int.TryParse(parts[1], out var stock) &&
                        int.TryParse(parts[2], out var reserved))
                    {
                        onQuantityUpdate(id, stock, reserved);
                    }
                    else
                    {
                        Debug.WriteLine($"Malformed SSE data: {line}");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("SSE monitoring cancelled by token");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during SSE monitoring: {ex.Message}");
            }
        }
    }
}
