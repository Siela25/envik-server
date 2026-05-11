namespace EnvikServer.Core.DTOs.Auth;

public record AuthResponse(
    string accessToken,
    string refreshToken
);