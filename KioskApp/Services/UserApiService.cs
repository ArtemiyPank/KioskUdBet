using System.Diagnostics;
using System.Net.Http.Json;
using KioskApp.Models;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Globalization;
using System.Text;
using System.Net;
using KioskApp.Helpers;
using Microsoft.Maui.ApplicationModel.Communication;

namespace KioskApp.Services
{
    public class UserApiService : IUserApiService
    {
        private readonly AppState _appState;
        private readonly HttpClient _httpClient;


        public UserApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _appState = DependencyService.Get<AppState>();
        }

        // Метод для отправки запросов с автоматическим обновлением токенов
        public async Task<HttpResponseMessage> SendRequestAsync(Func<HttpRequestMessage> createRequest, bool isSseRequest = false)
        {
            //Debug.WriteLine("In SendRequestAsync");
            if (_appState.CurrentUser != null)
            {
                //Debug.WriteLine(":::::: flag 1 :::::: SendRequestAsync");

                await EnsureAccessToken();

                //Debug.WriteLine(":::::: flag 2 :::::: SendRequestAsync");

                var request = createRequest();
                
                //Debug.WriteLine(":::::: flag 3 :::::: SendRequestAsync");
                
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _appState.AccessToken);
               
                //Debug.WriteLine(":::::: flag 4 :::::: SendRequestAsync");

                if (!isSseRequest) return await _httpClient.SendAsync(request);
                
                //Debug.WriteLine(":::::: flag 5 :::::: SendRequestAsync");
                
                return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            }

            return await _httpClient.SendAsync(createRequest());
        }


        // Ensure access token is valid and refresh if necessary
        private async Task EnsureAccessToken()
        {
            if (IsTokenExpired(_appState.AccessToken))
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


        // Register a new user and retrieve access and refresh tokens
        public async Task<AuthResponse> RegisterUser(User user)
        {
            try
            {
                var response = await SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Post, "/api/users/register")
                    {
                        Content = JsonContent.Create(user)
                    };
                });

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

                    //// Fetch or create the last order for the user
                    //Debug.WriteLine($"Fetching or creating the last order for user with ID: {user.Id}");
                    //var orderResponse = await SendRequestAsync(() =>
                    //{
                    //    return new HttpRequestMessage(HttpMethod.Get, $"/api/order/user/{user.Id}/lastOrder");
                    //});

                    //// Логируем статус ответа
                    //Debug.WriteLine($"Order response status: {orderResponse.StatusCode}");

                    //var lastOrder = await orderResponse.Content.ReadFromJsonAsync<Order>();

                    //// Проверяем, что заказ был успешно получен и логируем
                    //if (lastOrder != null)
                    //{
                    //    Debug.WriteLine($"Fetched or created last order with ID: {lastOrder.Id} for user {user.Id}");
                    //    Preferences.Set("OrderId", lastOrder.Id);
                    //}
                    //else
                    //{
                    //    Debug.WriteLine($"Failed to fetch or create the last order for user {user.Id}");
                    //}


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
            //Debug.WriteLine("In AuthenticateUser");
            try
            {
                //Debug.WriteLine("--------- flag 1 --------- AuthenticateUser");
                var response = await SendRequestAsync(() =>
                {
                    return new HttpRequestMessage(HttpMethod.Post, "/api/users/authenticate")
                    {
                        Content = JsonContent.Create(new { email, password })
                    };
                });
                //Debug.WriteLine($"response: {response}");
                //Debug.WriteLine("--------- flag 2 --------- AuthenticateUser");

                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
                //Debug.WriteLine($"result: {result}");

                //Debug.WriteLine("--------- flag 3 --------- AuthenticateUser");

                if (result.IsSuccess)
                {
                    var accessTokens = response.Headers.GetValues("Access-Token");
                    var refreshTokens = response.Headers.GetValues("Refresh-Token");
                    var token = accessTokens.FirstOrDefault();
                    var refreshToken = refreshTokens.FirstOrDefault();
                    //Debug.WriteLine("--------- flag 4 --------- AuthenticateUser");

                    // Deserialize user data
                    string userJson = JsonSerializer.Serialize(result.Data);
                    var user = JsonSerializer.Deserialize<User>(userJson);

                    //Debug.WriteLine("--------- flag 5 --------- AuthenticateUser");

                    // Fetch or create the last order for the user
                    //Debug.WriteLine($"Fetching or creating the last order for user with ID: {user.Id}");
                    //var orderResponse = await SendRequestAsync(() =>
                    //{
                    //    return new HttpRequestMessage(HttpMethod.Get, $"/api/order/user/{user.Id}/lastOrder");
                    //});
                    //Debug.WriteLine("--------- flag 6 --------- AuthenticateUser");

                    // Логируем статус ответа
                    //Debug.WriteLine($"Order response status: {orderResponse.StatusCode}");
                    //Debug.WriteLine($"orderResponse: {orderResponse}");
                    //Debug.WriteLine($"orderResponse.Content: {orderResponse.Content.ToString()}");

                    //string lastOrderJson = JsonSerializer.Serialize(result.Data);
                    //Debug.WriteLine($"lastOrderJson: {lastOrderJson}");

                    //var lastOrder = JsonSerializer.Deserialize<User>(userJson);
                    //Debug.WriteLine($"lastOrder: {lastOrder}");

                    //var lastOrder = await orderResponse.Content.ReadFromJsonAsync<Order>();

                    //// Проверяем, что заказ был успешно получен и логируем
                    //if (lastOrder != null)
                    //{
                    //    Debug.WriteLine($"Fetched or created last order with ID: {lastOrder.Id} for user {user.Id}");
                    //    Preferences.Set("OrderId", lastOrder.Id);
                    //}
                    //else
                    //{
                    //    Debug.WriteLine($"Failed to fetch or create the last order for user {user.Id}");
                    //}


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
                var response = await SendRequestAsync(() =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, "/api/users/authenticateWithToken");
                    request.Headers.Add("Refresh-Token", refreshToken);
                    return request;
                });

                var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

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

                    //// Fetch or create the last order for the user
                    //Debug.WriteLine($"Fetching or creating the last order for user with ID: {user.Id}");
                    //var orderResponse = await SendRequestAsync(() =>
                    //{
                    //    return new HttpRequestMessage(HttpMethod.Get, $"/api/order/user/{user.Id}/lastOrder");
                    //});

                    //// Логируем статус ответа
                    //Debug.WriteLine($"Order response status: {orderResponse.StatusCode}");

                    //var lastOrder = await orderResponse.Content.ReadFromJsonAsync<Order>();

                    //// Проверяем, что заказ был успешно получен и логируем
                    //if (lastOrder != null)
                    //{
                    //    Debug.WriteLine($"Fetched or created last order with ID: {lastOrder.Id} for user {user.Id}");
                    //    Preferences.Set("OrderId", lastOrder.Id);
                    //}
                    //else
                    //{
                    //    Debug.WriteLine($"Failed to fetch or create the last order for user {user.Id}");
                    //}


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
                Debug.WriteLine($"Error in AuthenticateWithToken in UserApiService: {ex.Message}");
                return new AuthResponse
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        // Clear the authorization header
        public void ClearAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            Debug.WriteLine("Authorization header cleared in ApiService.");
        }

        // Refresh access and refresh tokens
        public async Task RefreshTokens()
        {
            try
            {
                var refreshToken = await SecureStorage.GetAsync("refresh_token");
                var response = await _httpClient.PostAsJsonAsync("/api/users/refresh", new { Token = _appState.AccessToken, RefreshToken = refreshToken });
                if (response.Headers.TryGetValues("Access-Token", out var newAccessTokens) &&
                    response.Headers.TryGetValues("Refresh-Token", out var newRefreshTokens))
                {
                    _appState.AccessToken = newAccessTokens.FirstOrDefault();

                    await SecureStorage.SetAsync("auth_token", newAccessTokens.FirstOrDefault());
                    await SecureStorage.SetAsync("refresh_token", newRefreshTokens.FirstOrDefault());
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

