using System.Security.Cryptography;
using System.Text;
using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Application.Users;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LMS_Assignment.Application.Auth;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserService _userService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUserService userService,
        ILogger<AuthService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _userService = userService;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(
        string email,
        string password,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email} from {IpAddress}", email, ipAddress);
            throw new InvalidCredentialsException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login rejected for deactivated user {UserId} ({Email}) from {IpAddress}", user.Id, user.Email, ipAddress);
            throw new ForbiddenAccessException("Your account has been disabled by the administrator.");
        }

        var result = IssueTokens(user, ipAddress);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} ({Email}) logged in from {IpAddress}", user.Id, user.Email, ipAddress);

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
            _logger.LogWarning("Rejected refresh token attempt from {IpAddress}", ipAddress);
            throw new InvalidCredentialsException("Invalid or expired refresh token.");
        }

        storedToken.RevokedAt = DateTime.UtcNow;

        var result = IssueTokens(storedToken.User, ipAddress);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} refreshed their access token from {IpAddress}", storedToken.User.Id, ipAddress);

        return result;
    }

    public async Task<AuthResult> RegisterAsync(
        string fullName,
        string email,
        string password,
        UserRole role,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        if (role != UserRole.Teacher && role != UserRole.Student)
        {
            throw new BusinessRuleException("Self-registration is only allowed for the Teacher and Student roles.");
        }

        var user = await _userService.CreateUserAsync(fullName, email, password, role, cancellationToken);

        var result = IssueTokens(user, ipAddress);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} ({Email}) self-registered as {Role} from {IpAddress}", user.Id, user.Email, user.Role, ipAddress);

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
