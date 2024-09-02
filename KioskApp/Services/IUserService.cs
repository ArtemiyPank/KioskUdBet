using KioskApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public interface IUserService
    {
        Task<ApiResponse> Register(User user);
        Task<ApiResponse> Authenticate(string username, string password);
        Task<User> AuthenticateWithToken();
        Task<List<User>> GetAllUsers();
        User GetCurrentUser();
        void SetCurrentUser(User user);
        Task ClearCurrentUserAsync();
        Task Logout();
    }
}
