using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace AuthNuget.Security;

public static class JwtFactory
{
    public static string CreateJwtToken(string username, string role, RsaSecurityKey securityKey)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role),
        };

        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

        var tokenHandler = new JwtSecurityTokenHandler();

        string jwt = tokenHandler.CreateEncodedJwt(new SecurityTokenDescriptor()
        {
            Issuer = "auth",
            Audience = "pfe",
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow.AddMinutes(-1),
            Expires = DateTime.UtcNow.AddHours(1),
            IssuedAt = DateTime.UtcNow,
            SigningCredentials = credentials
        });

        return jwt;
    }
}