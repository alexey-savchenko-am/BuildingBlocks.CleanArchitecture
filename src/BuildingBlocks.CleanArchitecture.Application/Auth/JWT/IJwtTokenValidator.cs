using System.Security.Claims;

namespace BuildingBlocks.CleanArchitecture.Application.Auth.JWT;

public interface IJwtTokenValidator
{
    bool TryValidate(TokenValidationPayload validationPayload, out ClaimsPrincipal? principal);
}
