using KioskAPI.Models;
using System.Security.Claims;

namespace KioskAPI.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        RefreshToken GenerateRefreshToken(User user);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        void ValidateToken(string token);
    }
}
