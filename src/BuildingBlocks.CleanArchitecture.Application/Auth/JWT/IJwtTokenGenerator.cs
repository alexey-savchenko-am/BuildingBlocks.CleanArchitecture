namespace BuildingBlocks.CleanArchitecture.Application.Auth.JWT;

public interface IJwtTokenGenerator
{
    AccessToken GenerateAccessToken(string accessKey, TokenPayload payload, TimeSpan? expiresAt);
    RefreshToken GenerateRefreshToken(string refreshKey, TokenPayload payload, TimeSpan? expiresAt);
}
