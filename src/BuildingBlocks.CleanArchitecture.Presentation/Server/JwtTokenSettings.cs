namespace BuildingBlocks.CleanArchitecture.Presentation.Server;

public record JwtTokenSettings(string Token, string Issuer, string Audience, bool RequireHttpsMetadata);