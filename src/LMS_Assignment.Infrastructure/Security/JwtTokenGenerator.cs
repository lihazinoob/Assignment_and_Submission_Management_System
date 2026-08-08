using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LMS_Assignment.Infrastructure.Security;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;

    public TimeSpan AccessTokenLifetime { get; }

    public TimeSpan RefreshTokenLifetime { get; }

    public JwtTokenGenerator(IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");

        _key = jwtSection["Key"]
            ?? throw new InvalidOperationException("Jwt:Key was not found in configuration.");
        _issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer was not found in configuration.");
        _audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience was not found in configuration.");

        AccessTokenLifetime = TimeSpan.FromMinutes(jwtSection.GetValue<double?>("AccessTokenExpiryMinutes") ?? 30);
        RefreshTokenLifetime = TimeSpan.FromDays(jwtSection.GetValue<double?>("RefreshTokenExpiryDays") ?? 7);
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(ClaimTypes.Name, user.FullName)
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(AccessTokenLifetime),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}
