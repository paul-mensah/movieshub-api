using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MoviesHub.Api.Configurations;
using MoviesHub.Api.Models.Response;
using Newtonsoft.Json;

namespace MoviesHub.Api.Helpers;

public static class TokenGenerator
{
    public static GenerateTokenResponse GenerateToken(this UserResponse user, BearerTokenConfig config)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user), "User data must not be null or empty");
        }

        if (config is null)
        {
            throw new ArgumentNullException(nameof(config), "Bearer token configuration must not be null or empty");
        }
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config.Key));
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new (ClaimTypes.Thumbprint, JsonConvert.SerializeObject(user))
        };

        var token = new JwtSecurityToken(
            config.Issuer,
            config.Audience,
            claims,
            now.AddMilliseconds(-30),
            now.AddHours(12),
            new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature));

        string? tokenString = tokenHandler.WriteToken(token);

        return new GenerateTokenResponse
        {
            Expiry  = token.Payload.Exp,
            BearerToken = tokenString
        };
    }
}