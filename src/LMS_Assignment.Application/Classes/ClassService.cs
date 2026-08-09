using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Application.Classes;

public class ClassService : IClassService
{
    private readonly IApplicationDbContext _context;

    public ClassService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Class> CreateAsync(string name, string academicYear, CancellationToken cancellationToken = default)
    {
        var alreadyExists = await _context.Classes.AnyAsync(
            c => c.Name == name && c.AcademicYear == academicYear,
            cancellationToken);

        if (alreadyExists)
        {
            throw new BusinessRuleException($"A class named '{name}' already exists for academic year '{academicYear}'.");
        }

        var @class = new Class
        {
            Id = Guid.NewGuid(),
            Name = name,
            AcademicYear = academicYear,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Classes.Add(@class);
        await _context.SaveChangesAsync(cancellationToken);

        return @class;
    }

    public async Task<List<Class>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Classes
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
