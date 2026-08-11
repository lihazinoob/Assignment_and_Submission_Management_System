using LMS_Assignment.Application.ClassSubjects;
using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Tests.ClassSubjects;

public class ClassSubjectServiceTests
{
    private static (ClassSubjectService Service, TestApplicationDbContext Context) CreateSut()
    {
        var context = TestApplicationDbContext.CreateNew();
        var service = new ClassSubjectService(context);

        return (service, context);
    }

    private static async Task<(Class Class, Subject Subject)> SeedClassAndSubjectAsync(TestApplicationDbContext context)
    {
        var @class = new Class { Id = Guid.NewGuid(), Name = "Grade 10 - Section A", AcademicYear = "2026" };
        var subject = new Subject { Id = Guid.NewGuid(), Name = "Mathematics", Code = "MATH101" };

        context.Classes.Add(@class);
        context.Subjects.Add(subject);
        await context.SaveChangesAsync();

        return (@class, subject);
    }

    [Fact]
    public async Task CreateAsync_WithValidClassAndSubject_CreatesLink()
    {
        var (service, context) = CreateSut();
        var (@class, subject) = await SeedClassAndSubjectAsync(context);

        var result = await service.CreateAsync(@class.Id, subject.Id);

        Assert.Equal(@class.Id, result.ClassId);
        Assert.Equal(subject.Id, result.SubjectId);

        var stored = await context.ClassSubjects.SingleAsync();
        Assert.Equal(result.Id, stored.Id);
    }

    [Fact]
    public async Task CreateAsync_WithUnknownClassId_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var (_, subject) = await SeedClassAndSubjectAsync(context);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CreateAsync(Guid.NewGuid(), subject.Id));
    }

    [Fact]
    public async Task CreateAsync_WithUnknownSubjectId_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var (@class, _) = await SeedClassAndSubjectAsync(context);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CreateAsync(@class.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateAsync_WithAlreadyLinkedPair_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var (@class, subject) = await SeedClassAndSubjectAsync(context);

        await service.CreateAsync(@class.Id, subject.Id);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CreateAsync(@class.Id, subject.Id));
    }

    [Fact]
    public async Task CreateAsync_WithDeactivatedClass_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var (@class, subject) = await SeedClassAndSubjectAsync(context);
        @class.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CreateAsync(@class.Id, subject.Id));
    }

    [Fact]
    public async Task DeactivateAsync_ThenActivateAsync_TogglesIsActive()
    {
        var (service, context) = CreateSut();
        var (@class, subject) = await SeedClassAndSubjectAsync(context);
        var classSubject = await service.CreateAsync(@class.Id, subject.Id);

        var deactivated = await service.DeactivateAsync(classSubject.Id);
        Assert.False(deactivated.IsActive);

        var activated = await service.ActivateAsync(classSubject.Id);
        Assert.True(activated.IsActive);
    }

    [Fact]
    public async Task DeactivateAsync_WithUnknownId_ThrowsBusinessRuleException()
    {
        var (service, _) = CreateSut();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.DeactivateAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_WithNoTeacherAssignments_RemovesLink()
    {
        var (service, context) = CreateSut();
        var (@class, subject) = await SeedClassAndSubjectAsync(context);
        var classSubject = await service.CreateAsync(@class.Id, subject.Id);

        await service.DeleteAsync(classSubject.Id);

        Assert.False(await context.ClassSubjects.AnyAsync(cs => cs.Id == classSubject.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithTeacherAssignment_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var (@class, subject) = await SeedClassAndSubjectAsync(context);
        var classSubject = await service.CreateAsync(@class.Id, subject.Id);

        var teacher = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Teacher",
            Email = $"{Guid.NewGuid()}@lms.demo",
            PasswordHash = "irrelevant",
            Role = Domain.Enums.UserRole.Teacher,
            IsActive = true
        };
        context.Users.Add(teacher);
        context.TeacherSubjectAssignments.Add(new TeacherSubjectAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = teacher.Id,
            ClassSubjectId = classSubject.Id
        });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.DeleteAsync(classSubject.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithUnknownId_ThrowsBusinessRuleException()
    {
        var (service, _) = CreateSut();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.DeleteAsync(Guid.NewGuid()));
    }
}
