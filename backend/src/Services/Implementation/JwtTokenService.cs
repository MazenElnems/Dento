using Dento.Models;
using Dento.Options;
using Dento.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Dento.Services.Implementation;

public class JwtTokenService : ITokenService
{
    private readonly JwtTokenSettings _jwtTokenSettings;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(UserManager<ApplicationUser> userManager, IOptions<JwtTokenSettings> jwtTokenSettings, ILogger<JwtTokenService> logger)
    {
        _jwtTokenSettings = jwtTokenSettings.Value;
        _logger = logger;
    }

    public async Task<AccessToken> GetAccessToken(ApplicationUser user, string role)
    {
        var secretKey = _jwtTokenSettings.SecretKey;
        var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var singingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Role, role)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtTokenSettings.ExpiresInMinutes),
            SigningCredentials = singingCredentials,
            Issuer = _jwtTokenSettings.Issuer,
            Audience = _jwtTokenSettings.Audience
        };

        var token = new JwtSecurityTokenHandler().CreateToken(tokenDescriptor);

        var accessToken = new AccessToken
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpirationDate = tokenDescriptor.Expires ?? DateTime.UtcNow.AddMinutes(_jwtTokenSettings.ExpiresInMinutes)
        };

        _logger.LogInformation(
            "JWT token generated for user {UserId} | Role: {Role} | ExpiresAt: {ExpiresAt}",
            user.Id, role, accessToken.ExpirationDate);

        return accessToken;
    }
}
