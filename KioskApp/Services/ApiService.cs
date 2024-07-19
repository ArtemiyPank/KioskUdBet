using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using KioskApp.Models;
using System.Collections.Generic;
using System.Text.Json;

namespace KioskApp.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(User, string)> RegisterUser(User user)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/users/register", user);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                var registeredUser = System.Text.Json.JsonSerializer.Deserialize<User>(result["user"].ToString());
                var token = result["token"].ToString();
                Debug.WriteLine($"Registered User: {registeredUser.Email}");
                return (registeredUser, token);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RegisterUser: {ex.Message}");
                throw;
            }
        }

        public async Task<(User, string)> AuthenticateUser(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/users/authenticate", new { email, password });
                response.EnsureSuccessStatusCode();

                //Распоковка ответа
                var resultJson = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonDocument.Parse(resultJson).RootElement;
                var userJson = jsonObject.GetProperty("user").GetRawText();
                var token = jsonObject.GetProperty("token").GetString();
                var user = JsonSerializer.Deserialize<User>(userJson);

                Debug.WriteLine($"AuthenticateUser user : {user.Email}");
                Debug.WriteLine($"AuthenticateUser token : {token}");

                return (user, token);
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"HTTP Error in AuthenticateWithToken: {httpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in AuthenticateWithToken: {ex.Message}");
                throw;
            }
        }


        public async Task<User> AuthenticateWithToken(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("/api/users/authenticateWithToken");

                response.EnsureSuccessStatusCode();

                var resultJson = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonDocument.Parse(resultJson).RootElement;
                var userJson = jsonObject.GetProperty("user").GetRawText();
                var user = JsonSerializer.Deserialize<User>(userJson);

                if (user == null)
                {
                    throw new InvalidOperationException("Failed to deserialize the user object.");
                }

                return user;
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






        public async Task<List<Product>> GetProducts()
        {
            return await _httpClient.GetFromJsonAsync<List<Product>>("/api/products");
        }

        public async Task<Order> PlaceOrder(Order order)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/orders", order);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Order>();
        }

        public Task UpdateUser(User user)
        {
            throw new NotImplementedException();
        }
    }
}
