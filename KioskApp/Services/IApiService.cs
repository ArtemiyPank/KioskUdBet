using KioskApp.Models;

namespace KioskApp.Services
{
    public interface IApiService
    {
        Task<(User, string, string)> RegisterUser(User user);
        Task<(User, string, string)> AuthenticateUser(string email, string password);
        Task<(User, string, string)> AuthenticateWithToken(string refreshToken);
        Task<List<Product>> GetProducts();
        Task<Order> PlaceOrder(Order order);
        Task UpdateUser(User user);
        void ClearAuthorizationHeader();
    }
}
