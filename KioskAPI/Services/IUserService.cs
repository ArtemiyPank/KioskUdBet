using KioskAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskAPI.Services
{
    public interface IUserService
    {
        Task<User?> Authenticate(string username, string password);
        Task<User> Register(User user);
        Task<List<User>> GetAllUsers();
        Task<User> GetUserByToken(string token);
        Task<bool> EmailExists(string email);
        Task<User> GetUserByEmail(string email);
    }
}
