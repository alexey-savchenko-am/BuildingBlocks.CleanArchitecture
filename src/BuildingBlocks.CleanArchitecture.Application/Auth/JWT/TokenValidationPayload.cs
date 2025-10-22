namespace BuildingBlocks.CleanArchitecture.Application.Auth.JWT;

public record TokenValidationPayload(string TokenToValidate, string Key, string Issuer, string Audience);
