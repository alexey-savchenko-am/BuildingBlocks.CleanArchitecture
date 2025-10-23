using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BuildingBlocks.CleanArchitecture.Application.Auth.JWT;

internal sealed class JwtTokenService
    : IJwtTokenGenerator
    , IJwtTokenValidator
{
    public AccessToken GenerateAccessToken(string accessKey, TokenPayload payload, TimeSpan? expiresAt)
    {
        var claims = BuildClaims(payload, includeRoles: true);

        // NOTE: access token lives 15 min by default
        var expDate = DateTime.UtcNow.Add(expiresAt ?? TimeSpan.FromMinutes(15));

        var token = new JwtSecurityToken(
            issuer: payload.Issuer,
            audience: payload.Audience,
            claims: claims,
            expires: expDate,
            signingCredentials: GenerateCredentials(accessKey));

        return new (Value: new JwtSecurityTokenHandler().WriteToken(token), ExpirationDate: expDate);
    }

    public RefreshToken GenerateRefreshToken(string refreshKey, TokenPayload payload, TimeSpan? expiresAt)
    {
        var claims = BuildClaims(payload, includeRoles: false);

        // NOTE: refresh token lives 30 days by default
        var expDate = DateTime.UtcNow.Add(expiresAt ?? TimeSpan.FromDays(30));

        var token = new JwtSecurityToken(
            issuer: payload.Issuer,
            audience: payload.Audience,
            claims: claims,
            expires: expDate,
            signingCredentials: GenerateCredentials(refreshKey));

        return new(Value: new JwtSecurityTokenHandler().WriteToken(token), ExpirationDate: expDate);
    }

    public bool TryValidate(TokenValidationPayload validationPayload, out ClaimsPrincipal? principal)
    {
        principal = null;

        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(validationPayload.Key));

        try
        {
            principal = handler.ValidateToken(
                validationPayload.TokenToValidate,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = validationPayload.Issuer,
                    ValidAudience = validationPayload.Audience,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                },
                out var validatedToken);

            if (validatedToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                principal = null;
                return false;
            }

            return true; 
        }
        catch
        {
            return false; 
        }
    }

    private static SigningCredentials GenerateCredentials(string key)
    {
        var ssk = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        return new SigningCredentials(ssk, SecurityAlgorithms.HmacSha256);
    }

    private static List<Claim> BuildClaims(TokenPayload payload, bool includeRoles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, payload.Subject),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (includeRoles && payload.Roles != null)
            claims.AddRange(payload.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

        if (includeRoles && payload.CustomClaims != null)
            claims.AddRange(payload.CustomClaims.Select(kv => new Claim(kv.Key, kv.Value)));

        return claims;
    }
}
