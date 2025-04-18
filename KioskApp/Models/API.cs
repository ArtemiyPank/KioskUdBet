namespace KioskApp.Models
{
    // Response model for product stock queries
    public class ProductStockResponse
    {
        // Total quantity of product in inventory
        public int Stock { get; set; }

        // Quantity of product currently reserved
        public int ReservedStock { get; set; }
    }

    // Response model for authentication endpoints
    public class AuthResponse
    {
        // True if authentication succeeded
        public bool IsSuccess { get; set; }

        // Informational or error message
        public string Message { get; set; }

        // Authenticated user details
        public User User { get; set; }

        // JWT access token for API calls
        public string AccessToken { get; set; }

        // JWT refresh token for renewing the session
        public string RefreshToken { get; set; }
    }

    // Generic API response wrapper for user-related calls
    public class ApiResponse
    {
        // True if the API call succeeded
        public bool IsSuccess { get; set; }

        // Message from the API (e.g., errors or confirmations)
        public string Message { get; set; }

        // Data payload (user object)
        public User Data { get; set; }

        // Return a concise string representation of this response
        public override string ToString() =>
            $"IsSuccess: {IsSuccess}\n" +
            $"Message: {Message}\n" +
            $"User: {Data.ToString}";
    }
}
