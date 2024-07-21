using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using KioskApp.Models;
using System.Collections.Generic;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

namespace KioskApp.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private string _refreshToken;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Register a new user and retrieve access and refresh tokens
        public async Task<(User, string, string)> RegisterUser(User user)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/users/register", user);
                response.EnsureSuccessStatusCode();

                // Parse the response to get user details and tokens
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                var registeredUser = JsonSerializer.Deserialize<User>(result["user"].ToString());
                var token = result["token"].ToString();
                var refreshToken = result["refreshToken"].ToString();

                // Set the new access token in the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                Debug.WriteLine($"Registered User: {registeredUser.Email}");
                return (registeredUser, token, refreshToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RegisterUser: {ex.Message}");
                throw;
            }
        }

        // Authenticate user using email and password, and retrieve tokens
        public async Task<(User, string, string)> AuthenticateUser(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/users/authenticate", new { email, password });
                response.EnsureSuccessStatusCode();

                // Parse the response to get tokens and user details
                var resultJson = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonDocument.Parse(resultJson).RootElement;

                var token = jsonObject.GetProperty("token").GetString();
                var refreshToken = jsonObject.GetProperty("refreshToken").GetString();
                var userJson = jsonObject.GetProperty("user").GetRawText();
                var user = JsonSerializer.Deserialize<User>(userJson);

                Debug.WriteLine($"AuthenticateUser user: {user.Email}");
                Debug.WriteLine($"AuthenticateUser token: {token}");

                // Set the new access token in the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return (user, token, refreshToken);
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"HTTP Error in AuthenticateUser: {httpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AuthenticateUser: {ex.Message}");
                throw;
            }
        }

        // Authenticate using refresh token and retrieve new tokens
        public async Task<(User, string, string)> AuthenticateWithToken(string refreshToken)
        {
            try
            {
                // Set the refresh token in the request header
                _httpClient.DefaultRequestHeaders.Add("Refresh-Token", refreshToken);

                // Send request to refresh tokens
                var response = await _httpClient.PostAsync("/api/users/authenticateWithToken", null);
                response.EnsureSuccessStatusCode();

                // Parse the response to get new tokens and user details
                var resultJson = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonDocument.Parse(resultJson).RootElement;

                Debug.WriteLine($"jsonObject: {jsonObject}");

                var token = jsonObject.GetProperty("token").GetString();
                var newRefreshToken = jsonObject.GetProperty("refreshToken").GetString();
                var userJson = jsonObject.GetProperty("user").GetRawText();
                var user = JsonSerializer.Deserialize<User>(userJson);

                if (user == null)
                {
                    throw new InvalidOperationException("Failed to deserialize the user object.");
                }

                // Set the new access token in the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return (user, token, newRefreshToken);
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"HTTP error in AuthenticateWithToken: {httpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AuthenticateWithToken: {ex.Message}");
                throw;
            }
        }

        // Retrieve list of products from the server
        public async Task<List<Product>> GetProducts()
        {
            return await _httpClient.GetFromJsonAsync<List<Product>>("/api/products");
        }

        // Place an order on the server
        public async Task<Order> PlaceOrder(Order order)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/orders", order);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }

        // Update user details (not implemented yet)
        public Task UpdateUser(User user)
        {
            throw new NotImplementedException();
        }

        // Ensure access token is valid and refresh if necessary
        private async Task EnsureAccessToken()
        {
            if (_accessToken == null || IsTokenExpired(_accessToken))
            {
                await RefreshTokens();
            }
        }

        // Check if the token is expired
        private bool IsTokenExpired(string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            return jwtToken.ValidTo < DateTime.UtcNow;
        }

        // Clear the authorization header
        public void ClearAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            Debug.WriteLine("Authorization header cleared in ApiService.");
        }

        // Refresh access and refresh tokens
        private async Task RefreshTokens()
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/users/refresh", new { Token = _accessToken, RefreshToken = _refreshToken });
                response.EnsureSuccessStatusCode();

                var resultJson = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonDocument.Parse(resultJson).RootElement;
                _accessToken = jsonObject.GetProperty("token").GetString();
                _refreshToken = jsonObject.GetProperty("refreshToken").GetString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing tokens: {ex.Message}");
                throw;
            }
        }
    }
}
