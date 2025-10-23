namespace BuildingBlocks.CleanArchitecture.Application.Auth.JWT;

public record AccessToken(string Value, DateTime ExpirationDate);
public record RefreshToken(string Value, DateTime ExpirationDate);