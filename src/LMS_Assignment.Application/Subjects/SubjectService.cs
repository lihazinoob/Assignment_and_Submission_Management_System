using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Extensions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Application.Common.Models;
using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Application.Subjects;

public class SubjectService : ISubjectService
{
    private readonly IApplicationDbContext _context;

    public SubjectService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Subject> CreateAsync(string name, string code, CancellationToken cancellationToken = default)
    {
        var codeTaken = await _context.Subjects.AnyAsync(s => s.Code == code, cancellationToken);
        if (codeTaken)
        {
            throw new BusinessRuleException($"A subject with code '{code}' already exists.");
        }

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync(cancellationToken);

        return subject;
    }

    public async Task<PagedResult<Subject>> GetAllAsync(PaginationQuery pagination, CancellationToken cancellationToken = default)
    {
        var query = _context.Subjects.OrderBy(s => s.Name);

        return await query.ToPagedResultAsync(pagination.Page, pagination.PageSize, cancellationToken);
    }
}
