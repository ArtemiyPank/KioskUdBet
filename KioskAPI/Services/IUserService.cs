using KioskAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskAPI.Services
{
    public interface IUserService
    {
        Task<User?> Authenticate(string username, string password);
        Task<(User, string, string)?> AuthenticateWithRefreshToken(string refreshToken);
        Task<User> Register(User user);
        Task<List<User>> GetAllUsers();
        Task<bool> EmailExists(string email);
        Task<User> GetUserByEmail(string email);
        Task SaveRefreshToken(int userId, string token, DateTime expiryDate);
        Task<bool> ValidateRefreshToken(User user, string refreshToken);
        Task RevokeRefreshToken(User user, string refreshToken);
        Task<User?> GetUserByRefreshToken(string refreshToken);
        Task<User> GetUserById(int userId);
    }
}
