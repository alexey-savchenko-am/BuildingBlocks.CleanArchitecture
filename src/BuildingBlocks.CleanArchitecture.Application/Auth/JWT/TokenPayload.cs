namespace BuildingBlocks.CleanArchitecture.Application.Auth.JWT;

public record TokenPayload(
    string Subject,
    string Issuer,
    string Audience,
    IEnumerable<string> Roles = null!,
    IDictionary<string, string>? CustomClaims = null);
