namespace BuildingBlocks.CleanArchitecture.Application.Auth.JWT;

public record JwtTokens(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);
