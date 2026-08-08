using System.Security.Cryptography;
using System.Text;
using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResult> LoginAsync(
        string email,
        string password,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !user.IsActive || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            throw new InvalidCredentialsException("Invalid email or password.");
        }

        var result = IssueTokens(user, ipAddress);
        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<AuthResult> RefreshTokenAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null
            || storedToken.RevokedAt is not null
            || storedToken.ExpiresAt <= DateTime.UtcNow
            || !storedToken.User.IsActive)
        {
            throw new InvalidCredentialsException("Invalid or expired refresh token.");
        }

        storedToken.RevokedAt = DateTime.UtcNow;

        var result = IssueTokens(storedToken.User, ipAddress);
        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }

    private AuthResult IssueTokens(User user, string? ipAddress)
    {
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        var accessTokenExpiresAt = DateTime.UtcNow.Add(_jwtTokenGenerator.AccessTokenLifetime);

        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.Add(_jwtTokenGenerator.RefreshTokenLifetime),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        });

        return new AuthResult(accessToken, refreshToken, accessTokenExpiresAt);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
