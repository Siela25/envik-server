using EnvikServer.Core.Entities;

namespace EnvikServer.Core.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task StoreRefreshTokenAsync(Guid userId, string token);
    Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken);
    Task DeleteRefreshTokenAsync(Guid userId);
}