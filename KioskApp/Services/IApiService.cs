using KioskApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public interface IApiService
    {
        Task<AuthResponse> RegisterUser(User user);
        Task<AuthResponse> AuthenticateUser(string email, string password);
        Task<AuthResponse> AuthenticateWithToken(string refreshToken);
        void ClearAuthorizationHeader();
        Task<List<Product>> GetProducts();
        Task<Product> AddProduct(Product product, Stream imageStream, string imageName);
        Task<Order> PlaceOrder(Order order);
    }
}
