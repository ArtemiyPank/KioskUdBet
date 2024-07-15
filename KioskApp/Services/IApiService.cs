using KioskApp.Models;

namespace KioskApp.Services
{
    public interface IApiService
    {
        Task<(User, string)> RegisterUser(User user);
        Task<(User, string)> AuthenticateUser(string username, string password);
        Task<User> AuthenticateWithToken(string token);
        Task<List<Product>> GetProducts();
        Task<Order> PlaceOrder(Order order);
        Task UpdateUser(User user);
    }
}
