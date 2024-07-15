using KioskApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public interface IUserService
    {
        Task<(User, string)> Authenticate(string username, string password);
        Task<(User, string)> Register(User user);
        Task<User> AuthenticateWithToken(string token);
        Task<List<User>> GetAllUsers();
        User GetCurrentUser();
        void SetCurrentUser(User user);
        Task ClearCurrentUserAsync();
    }
}
