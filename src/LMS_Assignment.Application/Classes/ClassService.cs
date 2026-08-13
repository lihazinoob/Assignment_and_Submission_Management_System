using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Extensions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Application.Common.Models;
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

    public async Task<PagedResult<Class>> GetAllAsync(PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        var query = _context.Classes.OrderBy(c => c.Name);

        return await query.ToPagedResultAsync(pagination.Page, pagination.PageSize, cancellationToken);
    }

    public async Task<Class> UpdateAsync(
        Guid classId,
        string name,
        string academicYear,
        CancellationToken cancellationToken = default)
    {
        var @class = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);
        if (@class is null)
        {
            throw new BusinessRuleException($"No class found with id '{classId}'.");
        }

        var duplicateExists = await _context.Classes.AnyAsync(
            c => c.Id != classId && c.Name == name && c.AcademicYear == academicYear,
            cancellationToken);

        if (duplicateExists)
        {
            throw new BusinessRuleException($"A class named '{name}' already exists for academic year '{academicYear}'.");
        }

        @class.Name = name;
        @class.AcademicYear = academicYear;
        await _context.SaveChangesAsync(cancellationToken);

        return @class;
    }

    public async Task<Class> DeactivateAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        var @class = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);
        if (@class is null)
        {
            throw new BusinessRuleException($"No class found with id '{classId}'.");
        }

        @class.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return @class;
    }

    public async Task<Class> ActivateAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        var @class = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);
        if (@class is null)
        {
            throw new BusinessRuleException($"No class found with id '{classId}'.");
        }

        @class.IsActive = true;
        await _context.SaveChangesAsync(cancellationToken);

        return @class;
    }

    public async Task DeleteAsync(Guid classId, CancellationToken cancellationToken = default)
    {
        var @class = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);
        if (@class is null)
        {
            throw new BusinessRuleException($"No class found with id '{classId}'.");
        }

        var hasClassSubjects = await _context.ClassSubjects.AnyAsync(cs => cs.ClassId == classId, cancellationToken);
        var hasEnrollments = await _context.StudentEnrollments.AnyAsync(e => e.ClassId == classId, cancellationToken);

        if (hasClassSubjects || hasEnrollments)
        {
            throw new BusinessRuleException(
                "This class has linked subjects or enrolled students and cannot be deleted. Deactivate it instead.");
        }

        _context.Classes.Remove(@class);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
