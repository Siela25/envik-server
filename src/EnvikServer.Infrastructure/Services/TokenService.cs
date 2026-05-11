using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EnvikServer.Core.Entities;
using EnvikServer.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace EnvikServer.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _configuration;

    public TokenService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _redis = redis;
        _configuration = configuration;
    }

    public string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["AccessTokenExpiryMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    public async Task StoreRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var db = _redis.GetDatabase();
        var expiry = TimeSpan.FromDays(double.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"]!));
        // outcome of this format is fact that with new login old session will be overwritten. A worry for another time
        await db.StringSetAsync($"refresh_token:{userId}", refreshToken, expiry); 
        
    }

    public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var db = _redis.GetDatabase();
        var stored = await db.StringGetAsync($"refresh_token:{userId}");
        return stored == refreshToken;
    }

    public async Task DeleteRefreshTokenAsync(Guid userId)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync($"refresh_token:{userId}");
    }

}