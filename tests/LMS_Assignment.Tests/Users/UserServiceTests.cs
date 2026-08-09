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
}
