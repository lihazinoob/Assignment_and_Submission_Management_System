using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.TeacherSubjectAssignments;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Tests.TeacherSubjectAssignments;

public class TeacherSubjectAssignmentServiceTests
{
    private static (TeacherSubjectAssignmentService Service, TestApplicationDbContext Context) CreateSut()
    {
        var context = TestApplicationDbContext.CreateNew();
        var service = new TeacherSubjectAssignmentService(context);

        return (service, context);
    }

    private static async Task<ClassSubject> SeedClassSubjectAsync(TestApplicationDbContext context)
    {
        var @class = new Class { Id = Guid.NewGuid(), Name = "Grade 10 - Section A", AcademicYear = "2026" };
        var subject = new Subject { Id = Guid.NewGuid(), Name = "Mathematics", Code = "MATH101" };
        var classSubject = new ClassSubject { Id = Guid.NewGuid(), ClassId = @class.Id, SubjectId = subject.Id };

        context.Classes.Add(@class);
        context.Subjects.Add(subject);
        context.ClassSubjects.Add(classSubject);
        await context.SaveChangesAsync();

        return classSubject;
    }

    private static async Task<User> SeedUserAsync(TestApplicationDbContext context, UserRole role)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Test User",
            Email = $"{Guid.NewGuid()}@lms.demo",
            PasswordHash = "irrelevant",
            Role = role,
            IsActive = true
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    [Fact]
    public async Task AssignTeacherAsync_WithValidTeacherAndClassSubject_CreatesAssignment()
    {
        var (service, context) = CreateSut();
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var classSubject = await SeedClassSubjectAsync(context);
        var admin = await SeedUserAsync(context, UserRole.Admin);

        var result = await service.AssignTeacherAsync(teacher.Id, classSubject.Id, admin.Id);

        Assert.Equal(teacher.Id, result.TeacherId);
        Assert.Equal(classSubject.Id, result.ClassSubjectId);
        Assert.Equal(admin.Id, result.AssignedBy);

        var stored = await context.TeacherSubjectAssignments.SingleAsync();
        Assert.Equal(result.Id, stored.Id);
    }

    [Fact]
    public async Task AssignTeacherAsync_WithNonTeacherUser_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var student = await SeedUserAsync(context, UserRole.Student);
        var classSubject = await SeedClassSubjectAsync(context);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.AssignTeacherAsync(student.Id, classSubject.Id, null));
    }

    [Fact]
    public async Task AssignTeacherAsync_WithUnknownTeacherId_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var classSubject = await SeedClassSubjectAsync(context);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.AssignTeacherAsync(Guid.NewGuid(), classSubject.Id, null));
    }

    [Fact]
    public async Task AssignTeacherAsync_WithUnknownClassSubjectId_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var teacher = await SeedUserAsync(context, UserRole.Teacher);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.AssignTeacherAsync(teacher.Id, Guid.NewGuid(), null));
    }

    [Fact]
    public async Task AssignTeacherAsync_WithAlreadyAssignedPair_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var classSubject = await SeedClassSubjectAsync(context);

        await service.AssignTeacherAsync(teacher.Id, classSubject.Id, null);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.AssignTeacherAsync(teacher.Id, classSubject.Id, null));
    }

    [Fact]
    public async Task AssignTeacherAsync_WithDeactivatedClass_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var classSubject = await SeedClassSubjectAsync(context);
        classSubject.Class.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.AssignTeacherAsync(teacher.Id, classSubject.Id, null));
    }

    [Fact]
    public async Task GetForCurrentUserAsync_AsAdmin_ReturnsAllAssignments()
    {
        var (service, context) = CreateSut();
        var teacherA = await SeedUserAsync(context, UserRole.Teacher);
        var teacherB = await SeedUserAsync(context, UserRole.Teacher);
        var classSubject = await SeedClassSubjectAsync(context);

        await service.AssignTeacherAsync(teacherA.Id, classSubject.Id, null);
        await service.AssignTeacherAsync(teacherB.Id, classSubject.Id, null);

        var result = await service.GetForCurrentUserAsync(Guid.NewGuid(), UserRole.Admin);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetForCurrentUserAsync_AsTeacher_ReturnsOnlyOwnAssignments()
    {
        var (service, context) = CreateSut();
        var teacherA = await SeedUserAsync(context, UserRole.Teacher);
        var teacherB = await SeedUserAsync(context, UserRole.Teacher);
        var classSubject = await SeedClassSubjectAsync(context);

        await service.AssignTeacherAsync(teacherA.Id, classSubject.Id, null);
        await service.AssignTeacherAsync(teacherB.Id, classSubject.Id, null);

        var result = await service.GetForCurrentUserAsync(teacherA.Id, UserRole.Teacher);

        Assert.Single(result);
        Assert.Equal(teacherA.Id, result[0].TeacherId);
    }
}
