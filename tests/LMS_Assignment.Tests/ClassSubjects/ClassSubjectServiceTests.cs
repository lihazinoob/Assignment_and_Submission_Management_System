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
}
