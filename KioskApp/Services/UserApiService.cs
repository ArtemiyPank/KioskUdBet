using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KioskApp.Helpers;
using KioskApp.Models;

namespace KioskApp.Services
{
    // Service for calling user-related API endpoints and handling token management
    public class UserApiService : IUserApiService
    {
        private readonly AppState _appState;
        private readonly HttpClient _httpClient;

        public UserApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _appState = DependencyService.Get<AppState>();
        }

        // Send an HTTP request, attaching the bearer token and refreshing it if expired
        public async Task<HttpResponseMessage> SendRequestAsync(
            Func<HttpRequestMessage> createRequest,
            bool isSseRequest = false)
        {
            if (_appState.CurrentUser != null)
            {
                await EnsureAccessTokenAsync();
                var request = createRequest();
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _appState.AccessToken);

                if (isSseRequest)
                    return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                return await _httpClient.SendAsync(request);
            }

            return await _httpClient.SendAsync(createRequest());
        }

        // Refresh the access token if it has expired
        private async Task EnsureAccessTokenAsync()
        {
            if (IsTokenExpired(_appState.AccessToken))
                await RefreshTokensAsync();
        }

        // Determine whether a JWT has passed its expiry time
        private bool IsTokenExpired(string token)
        {
            var jwt = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            return jwt?.ValidTo < DateTime.UtcNow;
        }

        // Register a new user and parse returned tokens and user data
        public async Task<AuthResponse> RegisterUserAsync(User user)
        {
            try
            {
                var response = await SendRequestAsync(() =>
                    new HttpRequestMessage(HttpMethod.Post, "/api/users/register")
                    {
                        Content = JsonContent.Create(user)
                    });

                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (result == null || !result.IsSuccess)
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = result?.Message ?? "Registration failed"
                    };
                }

                var accessToken = response.Headers.GetValues("Access-Token").FirstOrDefault();
                var refreshToken = response.Headers.GetValues("Refresh-Token").FirstOrDefault();

                var userJson = JsonSerializer.Serialize(result.Data);
                var registeredUser = JsonSerializer.Deserialize<User>(userJson);

                return new AuthResponse
                {
                    IsSuccess = true,
                    Message = result.Message,
                    User = registeredUser,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
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

        // Authenticate with email and password and retrieve tokens
        public async Task<AuthResponse> AuthenticateUserAsync(string email, string password)
        {
            try
            {
                var response = await SendRequestAsync(() =>
                    new HttpRequestMessage(HttpMethod.Post, "/api/users/authenticate")
                    {
                        Content = JsonContent.Create(new { email, password })
                    });

                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (result == null || !result.IsSuccess)
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = result?.Message ?? "Authentication failed"
                    };
                }

                var accessToken = response.Headers.GetValues("Access-Token").FirstOrDefault();
                var refreshToken = response.Headers.GetValues("Refresh-Token").FirstOrDefault();

                var userJson = JsonSerializer.Serialize(result.Data);
                var user = JsonSerializer.Deserialize<User>(userJson);

                return new AuthResponse
                {
                    IsSuccess = true,
                    Message = result.Message,
                    User = user,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
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

        // Authenticate using an existing refresh token and get new tokens
        public async Task<AuthResponse> AuthenticateWithTokenAsync(string refreshToken)
        {
            try
            {
                var response = await SendRequestAsync(() =>
                {
                    var req = new HttpRequestMessage(HttpMethod.Post, "/api/users/authenticateWithToken");
                    req.Headers.Add("Refresh-Token", refreshToken);
                    return req;
                });

                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (result == null || !result.IsSuccess)
                {
                    return new AuthResponse
                    {
                        IsSuccess = false,
                        Message = result?.Message ?? "Token authentication failed"
                    };
                }

                var accessToken = response.Headers.GetValues("Access-Token").FirstOrDefault();
                var newRefreshToken = response.Headers.GetValues("Refresh-Token").FirstOrDefault();

                var userJson = JsonSerializer.Serialize(result.Data);
                var user = JsonSerializer.Deserialize<User>(userJson);

                return new AuthResponse
                {
                    IsSuccess = true,
                    Message = result.Message,
                    User = user,
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken
                };
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

        // Remove the Authorization header from the HTTP client
        public void ClearAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            Debug.WriteLine("Authorization header cleared.");
        }

        // Call API to refresh both access and refresh tokens
        public async Task RefreshTokensAsync()
        {
            try
            {
                var storedRefresh = await SecureStorage.GetAsync("refresh_token");
                var response = await _httpClient.PostAsJsonAsync("/api/users/refresh", new
                {
                    Token = _appState.AccessToken,
                    RefreshToken = storedRefresh
                });

                if (response.Headers.TryGetValues("Access-Token", out var newAccess) &&
                    response.Headers.TryGetValues("Refresh-Token", out var newRefresh))
                {
                    _appState.AccessToken = newAccess.FirstOrDefault();
                    await SecureStorage.SetAsync("auth_token", newAccess.FirstOrDefault());
                    await SecureStorage.SetAsync("refresh_token", newRefresh.FirstOrDefault());
                }
                else
                {
                    throw new Exception("Token refresh response missing headers");
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
