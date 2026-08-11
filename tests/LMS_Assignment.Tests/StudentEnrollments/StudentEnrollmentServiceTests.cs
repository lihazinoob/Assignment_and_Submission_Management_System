using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.StudentEnrollments;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Tests.StudentEnrollments;

public class StudentEnrollmentServiceTests
{
    private static (StudentEnrollmentService Service, TestApplicationDbContext Context) CreateSut()
    {
        var context = TestApplicationDbContext.CreateNew();
        var service = new StudentEnrollmentService(context);

        return (service, context);
    }

    private static async Task<User> SeedUserAsync(TestApplicationDbContext context, UserRole role)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = $"{role} User",
            Email = $"{Guid.NewGuid()}@lms.demo",
            PasswordHash = "irrelevant",
            Role = role,
            IsActive = true
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private static async Task<Class> SeedClassAsync(TestApplicationDbContext context)
    {
        var @class = new Class { Id = Guid.NewGuid(), Name = "Grade 10 - Section A", AcademicYear = "2026" };
        context.Classes.Add(@class);
        await context.SaveChangesAsync();
        return @class;
    }

    [Fact]
    public async Task EnrollAsync_WithValidStudentAndClass_CreatesActiveEnrollment()
    {
        var (service, context) = CreateSut();
        var student = await SeedUserAsync(context, UserRole.Student);
        var @class = await SeedClassAsync(context);

        var result = await service.EnrollAsync(student.Id, @class.Id, "Roll-01");

        Assert.Equal(EnrollmentStatus.Active, result.Status);
        Assert.Equal("Roll-01", result.RollNumber);

        var stored = await context.StudentEnrollments.SingleAsync();
        Assert.Equal(result.Id, stored.Id);
    }

    [Fact]
    public async Task EnrollAsync_WithNonStudentUser_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var @class = await SeedClassAsync(context);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.EnrollAsync(teacher.Id, @class.Id, null));
    }

    [Fact]
    public async Task EnrollAsync_WithUnknownStudentId_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var @class = await SeedClassAsync(context);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.EnrollAsync(Guid.NewGuid(), @class.Id, null));
    }

    [Fact]
    public async Task EnrollAsync_WithUnknownClassId_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var student = await SeedUserAsync(context, UserRole.Student);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.EnrollAsync(student.Id, Guid.NewGuid(), null));
    }

    [Fact]
    public async Task EnrollAsync_WhenAlreadyEnrolled_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var student = await SeedUserAsync(context, UserRole.Student);
        var @class = await SeedClassAsync(context);

        await service.EnrollAsync(student.Id, @class.Id, null);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.EnrollAsync(student.Id, @class.Id, null));
    }

    [Fact]
    public async Task EnrollAsync_WithDeactivatedClass_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var student = await SeedUserAsync(context, UserRole.Student);
        var @class = await SeedClassAsync(context);
        @class.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.EnrollAsync(student.Id, @class.Id, null));
    }
}
