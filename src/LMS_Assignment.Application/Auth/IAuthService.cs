using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Application.Auth;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(
        string email,
        string password,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<AuthResult> RefreshTokenAsync(
        string refreshToken,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<AuthResult> RegisterAsync(
        string fullName,
        string email,
        string password,
        UserRole role,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
