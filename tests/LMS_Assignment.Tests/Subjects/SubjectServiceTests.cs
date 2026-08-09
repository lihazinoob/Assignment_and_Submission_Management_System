using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Subjects;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Tests.Subjects;

public class SubjectServiceTests
{
    private static (SubjectService Service, TestApplicationDbContext Context) CreateSut()
    {
        var context = TestApplicationDbContext.CreateNew();
        var service = new SubjectService(context);

        return (service, context);
    }

    [Fact]
    public async Task CreateAsync_WithNewCode_CreatesSubject()
    {
        var (service, context) = CreateSut();

        var result = await service.CreateAsync("Mathematics", "MATH101");

        Assert.Equal("Mathematics", result.Name);
        Assert.Equal("MATH101", result.Code);
        Assert.True(result.IsActive);

        var stored = await context.Subjects.SingleAsync();
        Assert.Equal(result.Id, stored.Id);
    }

    [Fact]
    public async Task CreateAsync_WithAlreadyTakenCode_ThrowsBusinessRuleException()
    {
        var (service, _) = CreateSut();

        await service.CreateAsync("Mathematics", "MATH101");

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => service.CreateAsync("Math Advanced", "MATH101"));
    }
}
