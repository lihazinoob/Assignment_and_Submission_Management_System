using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Application.Users;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LMS_Assignment.Tests.Users;

public class UserServiceTests
{
    private static (UserService Service, TestApplicationDbContext Context) CreateSut()
    {
        var context = TestApplicationDbContext.CreateNew();

        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed-password");

        var service = new UserService(context, passwordHasher.Object);

        return (service, context);
    }

    [Fact]
    public async Task CreateUserAsync_WithNewEmail_CreatesUserWithHashedPassword()
    {
        var (service, context) = CreateSut();

        var user = await service.CreateUserAsync("Jane Doe", "jane.doe@lms.demo", "TempPass123", UserRole.Teacher);

        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.Equal(UserRole.Teacher, user.Role);
        Assert.True(user.IsActive);

        var stored = await context.Users.SingleAsync();
        Assert.Equal("jane.doe@lms.demo", stored.Email);
    }

    [Fact]
    public async Task CreateUserAsync_WithAlreadyTakenEmail_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();

        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Existing User",
            Email = "taken@lms.demo",
            PasswordHash = "irrelevant",
            Role = UserRole.Student,
            IsActive = true
        });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CreateUserAsync("New Person", "taken@lms.demo", "AnotherPass123", UserRole.Student));
    }

    [Fact]
    public async Task GetUsersAsync_WithRoleFilter_ReturnsOnlyMatchingRoleOrderedByName()
    {
        var (service, context) = CreateSut();

        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), FullName = "Zed Teacher", Email = "zed@lms.demo", PasswordHash = "x", Role = UserRole.Teacher, IsActive = true },
            new User { Id = Guid.NewGuid(), FullName = "Amy Teacher", Email = "amy@lms.demo", PasswordHash = "x", Role = UserRole.Teacher, IsActive = true },
            new User { Id = Guid.NewGuid(), FullName = "Some Student", Email = "student@lms.demo", PasswordHash = "x", Role = UserRole.Student, IsActive = true });
        await context.SaveChangesAsync();

        var teachers = await service.GetUsersAsync(new UserFilter { Role = UserRole.Teacher });

        Assert.Equal(2, teachers.TotalCount);
        Assert.Equal(["Amy Teacher", "Zed Teacher"], teachers.Items.Select(u => u.FullName));
    }

    [Fact]
    public async Task GetUsersAsync_WithoutRoleFilter_ReturnsAllUsers()
    {
        var (service, context) = CreateSut();

        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), FullName = "A Teacher", Email = "a@lms.demo", PasswordHash = "x", Role = UserRole.Teacher, IsActive = true },
            new User { Id = Guid.NewGuid(), FullName = "B Student", Email = "b@lms.demo", PasswordHash = "x", Role = UserRole.Student, IsActive = true });
        await context.SaveChangesAsync();

        var users = await service.GetUsersAsync(new UserFilter());

        Assert.Equal(2, users.TotalCount);
    }

    [Fact]
    public async Task GetUsersAsync_WithPaging_ReturnsRequestedPageAndTotalCount()
    {
        var (service, context) = CreateSut();

        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), FullName = "A User", Email = "a@lms.demo", PasswordHash = "x", Role = UserRole.Student, IsActive = true },
            new User { Id = Guid.NewGuid(), FullName = "B User", Email = "b@lms.demo", PasswordHash = "x", Role = UserRole.Student, IsActive = true },
            new User { Id = Guid.NewGuid(), FullName = "C User", Email = "c@lms.demo", PasswordHash = "x", Role = UserRole.Student, IsActive = true });
        await context.SaveChangesAsync();

        var result = await service.GetUsersAsync(new UserFilter { Page = 2, PageSize = 2 });

        Assert.Single(result.Items);
        Assert.Equal("C User", result.Items[0].FullName);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task GetUsersAsync_WithIsActiveFilter_ReturnsOnlyMatching()
    {
        var (service, context) = CreateSut();

        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), FullName = "Active User", Email = "active@lms.demo", PasswordHash = "x", Role = UserRole.Student, IsActive = true },
            new User { Id = Guid.NewGuid(), FullName = "Inactive User", Email = "inactive@lms.demo", PasswordHash = "x", Role = UserRole.Student, IsActive = false });
        await context.SaveChangesAsync();

        var result = await service.GetUsersAsync(new UserFilter { IsActive = false });

        var single = Assert.Single(result.Items);
        Assert.Equal("Inactive User", single.FullName);
    }

    [Fact]
    public async Task GetUsersAsync_WithSearchFilter_MatchesNameOrEmail()
    {
        var (service, context) = CreateSut();

        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), FullName = "Jamie Rivera", Email = "jamie@lms.demo", PasswordHash = "x", Role = UserRole.Student, IsActive = true },
            new User { Id = Guid.NewGuid(), FullName = "Someone Else", Email = "someone@lms.demo", PasswordHash = "x", Role = UserRole.Student, IsActive = true });
        await context.SaveChangesAsync();

        var result = await service.GetUsersAsync(new UserFilter { Search = "jamie" });

        var single = Assert.Single(result.Items);
        Assert.Equal("Jamie Rivera", single.FullName);
    }

    [Fact]
    public async Task DeactivateUserAsync_WithExistingUser_SetsIsActiveFalse()
    {
        var (service, context) = CreateSut();

        var target = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Target Teacher",
            Email = "target@lms.demo",
            PasswordHash = "x",
            Role = UserRole.Teacher,
            IsActive = true
        };
        context.Users.Add(target);
        await context.SaveChangesAsync();

        var result = await service.DeactivateUserAsync(target.Id, Guid.NewGuid());

        Assert.False(result.IsActive);

        var stored = await context.Users.SingleAsync(u => u.Id == target.Id);
        Assert.False(stored.IsActive);
    }

    [Fact]
    public async Task DeactivateUserAsync_WithOwnAccount_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();

        var admin = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Demo Admin",
            Email = "admin@lms.demo",
            PasswordHash = "x",
            Role = UserRole.Admin,
            IsActive = true
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.DeactivateUserAsync(admin.Id, admin.Id));

        var stored = await context.Users.SingleAsync(u => u.Id == admin.Id);
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task DeactivateUserAsync_WithUnknownUserId_ThrowsBusinessRuleException()
    {
        var (service, _) = CreateSut();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.DeactivateUserAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task ActivateUserAsync_WithDeactivatedUser_SetsIsActiveTrue()
    {
        var (service, context) = CreateSut();

        var target = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Target Teacher",
            Email = "target@lms.demo",
            PasswordHash = "x",
            Role = UserRole.Teacher,
            IsActive = false
        };
        context.Users.Add(target);
        await context.SaveChangesAsync();

        var result = await service.ActivateUserAsync(target.Id);

        Assert.True(result.IsActive);

        var stored = await context.Users.SingleAsync(u => u.Id == target.Id);
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task ActivateUserAsync_WithUnknownUserId_ThrowsBusinessRuleException()
    {
        var (service, _) = CreateSut();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.ActivateUserAsync(Guid.NewGuid()));
    }
}
