using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Application.TeacherSubjectAssignments;

public class TeacherSubjectAssignmentService : ITeacherSubjectAssignmentService
{
    private readonly IApplicationDbContext _context;

    public TeacherSubjectAssignmentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherSubjectAssignment> AssignTeacherAsync(
        Guid teacherId,
        Guid classSubjectId,
        Guid? assignedBy,
        CancellationToken cancellationToken = default)
    {
        var teacher = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == teacherId, cancellationToken);

        if (teacher is null || teacher.Role != UserRole.Teacher)
        {
            throw new BusinessRuleException($"User '{teacherId}' does not belong to a Teacher account.");
        }

        var classSubject = await _context.ClassSubjects
            .Include(cs => cs.Class)
            .Include(cs => cs.Subject)
            .FirstOrDefaultAsync(cs => cs.Id == classSubjectId, cancellationToken);

        if (classSubject is null)
        {
            throw new BusinessRuleException($"No class-subject link found with id '{classSubjectId}'.");
        }

        if (!classSubject.Class.IsActive)
        {
            throw new BusinessRuleException("This class has been deactivated. No new activity is allowed.");
        }

        var alreadyAssigned = await _context.TeacherSubjectAssignments.AnyAsync(
            t => t.TeacherId == teacherId && t.ClassSubjectId == classSubjectId,
            cancellationToken);

        if (alreadyAssigned)
        {
            throw new BusinessRuleException("This teacher is already assigned to this class-subject.");
        }

        var assignment = new TeacherSubjectAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = teacherId,
            ClassSubjectId = classSubjectId,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow,
            Teacher = teacher,
            ClassSubject = classSubject
        };

        _context.TeacherSubjectAssignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return assignment;
    }

    public async Task<List<TeacherSubjectAssignment>> GetForCurrentUserAsync(
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var query = _context.TeacherSubjectAssignments
            .Include(t => t.Teacher)
            .Include(t => t.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(t => t.ClassSubject).ThenInclude(cs => cs.Subject)
            .AsQueryable();

        if (role == UserRole.Teacher)
        {
            query = query.Where(t => t.TeacherId == userId);
        }

        return await query.OrderBy(t => t.AssignedAt).ToListAsync(cancellationToken);
    }
}
