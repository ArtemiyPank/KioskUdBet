using System.Diagnostics;
using System.Net.Http.Json;
using KioskApp.Models;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Globalization;
using System.Text;

namespace KioskApp.Services
{
    public class UserApiService : IUserApiService
    {
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private string _refreshToken;

        public UserApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Register a new user and retrieve access and refresh tokens
        public async Task<AuthResponse> RegisterUser(User user)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/users/register", user);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

                Debug.WriteLine($"result - {result.ToString}");
                Debug.WriteLine($"result.IsSuccess - {result.IsSuccess}");
                Debug.WriteLine($"result.Message - {result.Message}");


                if (result.IsSuccess)
                {

                    var accessTokens = response.Headers.GetValues("Access-Token");
                    var refreshTokens = response.Headers.GetValues("Refresh-Token");
                    var token = accessTokens.FirstOrDefault();
                    var refreshToken = refreshTokens.FirstOrDefault();

                    // Serialize the Data to JSON string and then parse it as JsonElement
                    string userJson = JsonSerializer.Serialize(result.Data);
                    JsonElement jsonElement = JsonDocument.Parse(userJson).RootElement;

                    // Deserialize JsonElement to User object
                    var registeredUser = JsonSerializer.Deserialize<User>(jsonElement.GetRawText());

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    return new AuthResponse
                    {
                        IsSuccess = true,
                        Message = result.Message,
                        User = registeredUser,
                        AccessToken = token,
                        RefreshToken = refreshToken
                    };
                }
                else
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = result.Message
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RegisterUser: {ex.Message}");
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        // Authenticate user using email and password, and retrieve tokens
        public async Task<AuthResponse> AuthenticateUser(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/users/authenticate", new { email, password });
                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

                if (result.IsSuccess)
                {
                    var accessTokens = response.Headers.GetValues("Access-Token");
                    var refreshTokens = response.Headers.GetValues("Refresh-Token");
                    var token = accessTokens.FirstOrDefault();
                    var refreshToken = refreshTokens.FirstOrDefault();

                    // Serialize the Data to JSON string and then parse it as JsonElement
                    string userJson = JsonSerializer.Serialize(result.Data);
                    JsonElement jsonElement = JsonDocument.Parse(userJson).RootElement;

                    // Deserialize JsonElement to User object
                    var user = JsonSerializer.Deserialize<User>(jsonElement.GetRawText());

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    return new AuthResponse
                    {
                        IsSuccess = true,
                        Message = result.Message,
                        User = user,
                        AccessToken = token,
                        RefreshToken = refreshToken
                    };
                }
                else
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = result.Message
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AuthenticateUser: {ex.Message}");
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        // Authenticate using refresh token and retrieve new tokens
        public async Task<AuthResponse> AuthenticateWithToken(string refreshToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Add("Refresh-Token", refreshToken);
                var response = await _httpClient.PostAsync("/api/users/authenticateWithToken", null);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

                Debug.WriteLine($"result.Message - {result.Message}");
                Debug.WriteLine($"result.Message - {result.Message}");

                if (result.IsSuccess)
                {
                    var accessTokens = response.Headers.GetValues("Access-Token");
                    var refreshTokens = response.Headers.GetValues("Refresh-Token");
                    var token = accessTokens.FirstOrDefault();
                    var newRefreshToken = refreshTokens.FirstOrDefault();

                    // Serialize the Data to JSON string and then parse it as JsonElement
                    string userJson = JsonSerializer.Serialize(result.Data);
                    JsonElement jsonElement = JsonDocument.Parse(userJson).RootElement;

                    // Deserialize JsonElement to User object
                    var user = JsonSerializer.Deserialize<User>(jsonElement.GetRawText());

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    return new AuthResponse
                    {
                        IsSuccess = true,
                        Message = result.Message,
                        User = user,
                        AccessToken = token,
                        RefreshToken = newRefreshToken
                    };
                }
                else
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = result.Message
                    };
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AuthenticateWithToken: {ex.Message}");
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
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
                if (response.Headers.TryGetValues("Access-Token", out var accessTokens) &&
                    response.Headers.TryGetValues("Refresh-Token", out var refreshTokens))
                {
                    _accessToken = accessTokens.FirstOrDefault();
                    _refreshToken = refreshTokens.FirstOrDefault();
                }
                else
                {
                    throw new Exception("Failed to retrieve tokens");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error refreshing tokens: {ex.Message}");
                throw;
            }
        }

    }
}

