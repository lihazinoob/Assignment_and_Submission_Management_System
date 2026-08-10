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

        var teachers = await service.GetUsersAsync(UserRole.Teacher);

        Assert.Equal(2, teachers.Count);
        Assert.Equal(["Amy Teacher", "Zed Teacher"], teachers.Select(u => u.FullName));
    }

    [Fact]
    public async Task GetUsersAsync_WithoutRoleFilter_ReturnsAllUsers()
    {
        var (service, context) = CreateSut();

        context.Users.AddRange(
            new User { Id = Guid.NewGuid(), FullName = "A Teacher", Email = "a@lms.demo", PasswordHash = "x", Role = UserRole.Teacher, IsActive = true },
            new User { Id = Guid.NewGuid(), FullName = "B Student", Email = "b@lms.demo", PasswordHash = "x", Role = UserRole.Student, IsActive = true });
        await context.SaveChangesAsync();

        var users = await service.GetUsersAsync(null);

        Assert.Equal(2, users.Count);
    }
}
