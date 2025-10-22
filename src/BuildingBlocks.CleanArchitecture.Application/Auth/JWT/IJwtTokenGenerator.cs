namespace BuildingBlocks.CleanArchitecture.Application.Auth.JWT;

public interface IJwtTokenGenerator
{
    JwtTokens GenerateTokens(string accessKey, string refreshKey, TokenPayload payload);
    (string token, DateTime expiresAt) GenerateAccessToken(string accessKey, TokenPayload payload);
    (string token, DateTime expiresAt) GenerateRefreshToken(string refreshKey, TokenPayload payload);
}
