using LMS_Assignment.Application.Classes;
using LMS_Assignment.Application.Common.Exceptions;
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
}
