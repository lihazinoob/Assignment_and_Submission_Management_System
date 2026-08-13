using LMS_Assignment.Application.Classes;
using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Models;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Tests.Classes;

public class ClassServiceTests
{
    private static (ClassService Service, TestApplicationDbContext Context) CreateSut()
    {
        var context = TestApplicationDbContext.CreateNew();
        var service = new ClassService(context);

        return (service, context);
    }

    [Fact]
    public async Task CreateAsync_WithNewNameAndYear_CreatesClass()
    {
        var (service, context) = CreateSut();

        var result = await service.CreateAsync("Grade 10 - Section A", "2026");

        Assert.Equal("Grade 10 - Section A", result.Name);
        Assert.Equal("2026", result.AcademicYear);
        Assert.True(result.IsActive);

        var stored = await context.Classes.SingleAsync();
        Assert.Equal(result.Id, stored.Id);
    }

    [Fact]
    public async Task CreateAsync_WithSameNameAndYear_ThrowsBusinessRuleException()
    {
        var (service, _) = CreateSut();

        await service.CreateAsync("Grade 10 - Section A", "2026");

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CreateAsync("Grade 10 - Section A", "2026"));
    }

    [Fact]
    public async Task CreateAsync_WithSameNameDifferentYear_Succeeds()
    {
        var (service, context) = CreateSut();

        await service.CreateAsync("Grade 10 - Section A", "2026");
        await service.CreateAsync("Grade 10 - Section A", "2027");

        Assert.Equal(2, await context.Classes.CountAsync());
    }

    [Fact]
    public async Task UpdateAsync_WithNewNameAndYear_UpdatesClass()
    {
        var (service, context) = CreateSut();
        var @class = await service.CreateAsync("Grade 10 - Section A", "2026");

        var result = await service.UpdateAsync(@class.Id, "Grade 10 - Section B", "2027");

        Assert.Equal("Grade 10 - Section B", result.Name);
        Assert.Equal("2027", result.AcademicYear);

        var stored = await context.Classes.SingleAsync(c => c.Id == @class.Id);
        Assert.Equal("Grade 10 - Section B", stored.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithUnknownClassId_ThrowsBusinessRuleException()
    {
        var (service, _) = CreateSut();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.UpdateAsync(Guid.NewGuid(), "Grade 10 - Section A", "2026"));
    }

    [Fact]
    public async Task UpdateAsync_ToNameAlreadyUsedByAnotherClass_ThrowsBusinessRuleException()
    {
        var (service, _) = CreateSut();
        await service.CreateAsync("Grade 10 - Section A", "2026");
        var other = await service.CreateAsync("Grade 11 - Section A", "2026");

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.UpdateAsync(other.Id, "Grade 10 - Section A", "2026"));
    }

    [Fact]
    public async Task DeactivateAsync_ThenActivateAsync_TogglesIsActive()
    {
        var (service, context) = CreateSut();
        var @class = await service.CreateAsync("Grade 10 - Section A", "2026");

        var deactivated = await service.DeactivateAsync(@class.Id);
        Assert.False(deactivated.IsActive);

        var reactivated = await service.ActivateAsync(@class.Id);
        Assert.True(reactivated.IsActive);

        var stored = await context.Classes.SingleAsync(c => c.Id == @class.Id);
        Assert.True(stored.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_WithEmptyClass_RemovesClass()
    {
        var (service, context) = CreateSut();
        var @class = await service.CreateAsync("Grade 10 - Section A", "2026");

        await service.DeleteAsync(@class.Id);

        Assert.False(await context.Classes.AnyAsync(c => c.Id == @class.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithLinkedSubject_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var @class = await service.CreateAsync("Grade 10 - Section A", "2026");
        var subject = new Subject { Id = Guid.NewGuid(), Name = "Mathematics", Code = "MATH101" };
        context.Subjects.Add(subject);
        context.ClassSubjects.Add(new ClassSubject { Id = Guid.NewGuid(), ClassId = @class.Id, SubjectId = subject.Id });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DeleteAsync(@class.Id));

        Assert.True(await context.Classes.AnyAsync(c => c.Id == @class.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithEnrolledStudent_ThrowsBusinessRuleException()
    {
        var (service, context) = CreateSut();
        var @class = await service.CreateAsync("Grade 10 - Section A", "2026");
        var student = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Student One",
            Email = "student.one@lms.demo",
            PasswordHash = "irrelevant",
            Role = UserRole.Student
        };
        context.Users.Add(student);
        context.StudentEnrollments.Add(new StudentEnrollment { Id = Guid.NewGuid(), StudentId = student.Id, ClassId = @class.Id });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DeleteAsync(@class.Id));

        Assert.True(await context.Classes.AnyAsync(c => c.Id == @class.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithUnknownClassId_ThrowsBusinessRuleException()
    {
        var (service, _) = CreateSut();

        await Assert.ThrowsAsync<BusinessRuleException>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAllAsync_WithPaging_ReturnsRequestedPageAndTotalCount()
    {
        var (service, _) = CreateSut();

        await service.CreateAsync("Grade 9 - Section A", "2026");
        await service.CreateAsync("Grade 10 - Section A", "2026");
        await service.CreateAsync("Grade 11 - Section A", "2026");

        var result = await service.GetAllAsync(new PaginationQuery { Page = 2, PageSize = 2 });

        Assert.Single(result.Items);
        Assert.Equal(3, result.TotalCount);
    }
}
