using KioskApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KioskApp.Services
{
    public interface IUserApiService
    {
        Task<HttpResponseMessage> SendRequestAsync(Func<HttpRequestMessage> createRequest);
        Task<AuthResponse> RegisterUser(User user);
        Task<AuthResponse> AuthenticateUser(string email, string password);
        Task<AuthResponse> AuthenticateWithToken(string refreshToken);
        void ClearAuthorizationHeader();
        Task RefreshTokens();
    }
}
