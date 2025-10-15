using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BuildingBlocks.CleanArchitecture.Presentation.Middlewares;

public sealed class JwtAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtAuthorizationMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();

            AttachUserToContext(context, token);
        }

        await _next(context);
    }

    private void AttachUserToContext(HttpContext context, string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Auth:Key"]!);

        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = _configuration["Auth:Issuer"],
            ValidAudience = _configuration["Auth:Issuer"],
            ClockSkew = TimeSpan.Zero
        }, out var validatedToken);

        var jwtToken = (JwtSecurityToken)validatedToken;
        var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, "jwt");
        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        foreach (var roleClaim in roleClaims)
        {
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
        }
        context.User = new ClaimsPrincipal(claimsIdentity);
    }
}
