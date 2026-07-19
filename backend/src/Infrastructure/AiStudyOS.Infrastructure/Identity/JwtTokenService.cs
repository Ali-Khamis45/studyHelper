using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AiStudyOS.Application.Common.Interfaces;
using AiStudyOS.Application.Common.Options;
using AiStudyOS.Domain.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AiStudyOS.Infrastructure.Identity;

public class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    public AccessTokenResult GenerateAccessToken(User user)
    {
        var jwt = options.Value;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwt.AccessTokenLifetimeMinutes);

        // Minimum-claims by design: the only consumer (CurrentUserService) reads `sub`, and
        // GetMeQueryHandler re-fetches the user from the DB rather than trusting token claims —
        // email/name in the token would just be unused PII exposed to anyone who decodes it.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
