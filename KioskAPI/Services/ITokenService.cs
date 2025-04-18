using System.Security.Claims;
using KioskAPI.Models;

namespace KioskAPI.Services
{
    public interface ITokenService
    {
        // Generate a new JWT access token for the specified user
        string GenerateAccessToken(User user);

        // Create a refresh token entry for the specified user
        RefreshToken GenerateRefreshToken(User user);

        // Extract the claims principal from an expired access token
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);

        // Validate the integrity and expiry of the provided token
        void ValidateToken(string token);
    }
}
