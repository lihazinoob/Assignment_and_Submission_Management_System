using System.Security.Cryptography;
using System.Text;
using LMS_Assignment.Application.Auth;
using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Application.Users;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LMS_Assignment.Tests.Auth;

public class AuthServiceTests
{
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private static (AuthService Service, TestApplicationDbContext Context, User User, Mock<IJwtTokenGenerator> JwtMock) CreateSut(
        bool passwordMatches,
        bool isActive = true)
    {
        var context = TestApplicationDbContext.CreateNew();

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Demo Teacher",
            Email = "teacher@lms.demo",
            PasswordHash = "hashed-password",
            Role = UserRole.Teacher,
            IsActive = isActive
        };

        context.Users.Add(user);
        context.SaveChanges();

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher
            .Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(passwordMatches);
        passwordHasher
            .Setup(h => h.Hash(It.IsAny<string>()))
            .Returns("hashed-password");

        var jwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        jwtTokenGenerator.SetupGet(g => g.AccessTokenLifetime).Returns(AccessTokenLifetime);
        jwtTokenGenerator.SetupGet(g => g.RefreshTokenLifetime).Returns(RefreshTokenLifetime);
        jwtTokenGenerator.Setup(g => g.GenerateAccessToken(It.IsAny<User>())).Returns("fake-access-token");
        jwtTokenGenerator.Setup(g => g.GenerateRefreshToken()).Returns("fake-refresh-token");

        var userService = new UserService(context, passwordHasher.Object);

        var service = new AuthService(context, passwordHasher.Object, jwtTokenGenerator.Object, userService, NullLogger<AuthService>.Instance);

        return (service, context, user, jwtTokenGenerator);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokensAndPersistsHashedRefreshToken()
    {
        var (service, context, user, _) = CreateSut(passwordMatches: true);

        var result = await service.LoginAsync(user.Email, "correct-password", "127.0.0.1");

        Assert.Equal("fake-access-token", result.AccessToken);
        Assert.Equal("fake-refresh-token", result.RefreshToken);

        var storedToken = await context.RefreshTokens.SingleAsync();
        Assert.Equal(user.Id, storedToken.UserId);
        Assert.NotEqual("fake-refresh-token", storedToken.TokenHash);
        Assert.Equal("127.0.0.1", storedToken.CreatedByIp);
        Assert.Null(storedToken.RevokedAt);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsInvalidCredentialsException()
    {
        var (service, _, user, _) = CreateSut(passwordMatches: false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => service.LoginAsync(user.Email, "wrong-password", null));
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ThrowsInvalidCredentialsException()
    {
        var (service, _, _, _) = CreateSut(passwordMatches: true);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => service.LoginAsync("nobody@lms.demo", "whatever", null));
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUserAndCorrectPassword_ThrowsForbiddenAccessException()
    {
        var (service, _, user, _) = CreateSut(passwordMatches: true, isActive: false);

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => service.LoginAsync(user.Email, "correct-password", null));
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUserAndWrongPassword_ThrowsInvalidCredentialsException()
    {
        // Password is checked before the active-status check, so a wrong password never
        // reveals whether the account exists and is merely deactivated.
        var (service, _, user, _) = CreateSut(passwordMatches: false, isActive: false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => service.LoginAsync(user.Email, "wrong-password", null));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_RotatesTokenAndReturnsNewOnes()
    {
        var (service, context, user, jwtMock) = CreateSut(passwordMatches: true);

        var existingToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken("old-refresh-token"),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        context.RefreshTokens.Add(existingToken);
        await context.SaveChangesAsync();

        jwtMock.Setup(g => g.GenerateRefreshToken()).Returns("new-refresh-token");

        var result = await service.RefreshTokenAsync("old-refresh-token", "127.0.0.1");

        Assert.Equal("new-refresh-token", result.RefreshToken);

        var reloadedOldToken = await context.RefreshTokens.SingleAsync(t => t.Id == existingToken.Id);
        Assert.NotNull(reloadedOldToken.RevokedAt);

        var newToken = await context.RefreshTokens.SingleAsync(t => t.Id != existingToken.Id);
        Assert.Equal(user.Id, newToken.UserId);
        Assert.Equal(HashToken("new-refresh-token"), newToken.TokenHash);
        Assert.Null(newToken.RevokedAt);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithUnknownToken_ThrowsInvalidCredentialsException()
    {
        var (service, _, _, _) = CreateSut(passwordMatches: true);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => service.RefreshTokenAsync("never-issued-token", null));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ThrowsInvalidCredentialsException()
    {
        var (service, context, user, _) = CreateSut(passwordMatches: true);

        context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken("expired-token"),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => service.RefreshTokenAsync("expired-token", null));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithAlreadyRevokedToken_ThrowsInvalidCredentialsException()
    {
        var (service, context, user, _) = CreateSut(passwordMatches: true);

        context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken("revoked-token"),
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            RevokedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => service.RefreshTokenAsync("revoked-token", null));
    }

    [Fact]
    public async Task RegisterAsync_WithTeacherRole_CreatesUserAndReturnsTokens()
    {
        var (service, context, _, _) = CreateSut(passwordMatches: true);

        var result = await service.RegisterAsync(
            "New Teacher", "new.teacher@lms.demo", "TempPass123", UserRole.Teacher, "127.0.0.1");

        Assert.Equal("fake-access-token", result.AccessToken);
        Assert.Equal("fake-refresh-token", result.RefreshToken);

        var stored = await context.Users.SingleAsync(u => u.Email == "new.teacher@lms.demo");
        Assert.Equal(UserRole.Teacher, stored.Role);

        var storedToken = await context.RefreshTokens.SingleAsync(t => t.UserId == stored.Id);
        Assert.Equal("127.0.0.1", storedToken.CreatedByIp);
    }

    [Fact]
    public async Task RegisterAsync_WithStudentRole_CreatesUserAndReturnsTokens()
    {
        var (service, context, _, _) = CreateSut(passwordMatches: true);

        await service.RegisterAsync(
            "New Student", "new.student@lms.demo", "TempPass123", UserRole.Student, null);

        var stored = await context.Users.SingleAsync(u => u.Email == "new.student@lms.demo");
        Assert.Equal(UserRole.Student, stored.Role);
    }

    [Fact]
    public async Task RegisterAsync_WithAdminRole_ThrowsBusinessRuleException()
    {
        var (service, _, _, _) = CreateSut(passwordMatches: true);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.RegisterAsync("New Admin", "new.admin@lms.demo", "TempPass123", UserRole.Admin, null));
    }

    [Fact]
    public async Task RegisterAsync_WithAlreadyTakenEmail_ThrowsBusinessRuleException()
    {
        var (service, _, user, _) = CreateSut(passwordMatches: true);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.RegisterAsync("Someone Else", user.Email, "TempPass123", UserRole.Teacher, null));
    }
}
