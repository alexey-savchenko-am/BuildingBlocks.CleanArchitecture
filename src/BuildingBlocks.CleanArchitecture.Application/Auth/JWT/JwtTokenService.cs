using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BuildingBlocks.CleanArchitecture.Application.Auth.JWT;

internal sealed class JwtTokenService
    : IJwtTokenGenerator
    , IJwtTokenValidator
{
    public JwtTokens GenerateTokens(string accessKey, string refreshKey, TokenPayload payload)
    {
        var accessToken = GenerateAccessToken(accessKey, payload);
        var refreshToken = GenerateRefreshToken(refreshKey, payload);

        return new JwtTokens(
            accessToken.token,
            refreshToken.token,
            accessToken.expiresAt,
            refreshToken.expiresAt);
    }

    public (string token, DateTime expiresAt) GenerateAccessToken(string accessKey, TokenPayload payload)
    {
        var claims = BuildClaims(payload, includeRoles: true);

        // NOTE: access token lives 15 min by default
        var expiresAt = DateTime.UtcNow.Add(payload.ExpiresAt ?? TimeSpan.FromMinutes(15));

        var token = new JwtSecurityToken(
            issuer: payload.Issuer,
            audience: payload.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: GenerateCredentials(accessKey));

        return (token: new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public (string token, DateTime expiresAt) GenerateRefreshToken(string refreshKey, TokenPayload payload)
    {
        var claims = BuildClaims(payload, includeRoles: false);

        // NOTE: refresh token lives 30 days by default
        var expiresAt = DateTime.UtcNow.Add(payload.ExpiresAt ?? TimeSpan.FromDays(30));

        var token = new JwtSecurityToken(
            issuer: payload.Issuer,
            audience: payload.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: GenerateCredentials(refreshKey));

        return (token: new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
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
