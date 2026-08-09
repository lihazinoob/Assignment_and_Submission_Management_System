using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Application.ClassSubjects;

public class ClassSubjectService : IClassSubjectService
{
    private readonly IApplicationDbContext _context;

    public ClassSubjectService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClassSubject> CreateAsync(Guid classId, Guid subjectId, CancellationToken cancellationToken = default)
    {
        var @class = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);
        if (@class is null)
        {
            throw new BusinessRuleException($"No class found with id '{classId}'.");
        }

        var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId, cancellationToken);
        if (subject is null)
        {
            throw new BusinessRuleException($"No subject found with id '{subjectId}'.");
        }

        var alreadyLinked = await _context.ClassSubjects.AnyAsync(
            cs => cs.ClassId == classId && cs.SubjectId == subjectId,
            cancellationToken);

        if (alreadyLinked)
        {
            throw new BusinessRuleException("This subject is already linked to this class.");
        }

        var classSubject = new ClassSubject
        {
            Id = Guid.NewGuid(),
            ClassId = classId,
            SubjectId = subjectId,
            CreatedAt = DateTime.UtcNow,
            Class = @class,
            Subject = subject
        };

        _context.ClassSubjects.Add(classSubject);
        await _context.SaveChangesAsync(cancellationToken);

        return classSubject;
    }

    public async Task<List<ClassSubject>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ClassSubjects
            .Include(cs => cs.Class)
            .Include(cs => cs.Subject)
            .OrderBy(cs => cs.Class.Name)
            .ToListAsync(cancellationToken);
    }
}
