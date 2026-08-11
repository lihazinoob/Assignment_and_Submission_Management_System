using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Application.Assignments;

public class AssignmentService : IAssignmentService
{
    private readonly IApplicationDbContext _context;

    public AssignmentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Assignment> CreateAsync(
        Guid teacherSubjectAssignmentId,
        string title,
        string? description,
        DateTime deadline,
        decimal maxMarks,
        bool allowResubmission,
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        var teacherSubjectAssignment = await _context.TeacherSubjectAssignments
            .Include(t => t.Teacher)
            .Include(t => t.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(t => t.ClassSubject).ThenInclude(cs => cs.Subject)
            .FirstOrDefaultAsync(t => t.Id == teacherSubjectAssignmentId, cancellationToken);

        if (teacherSubjectAssignment is null)
        {
            throw new BusinessRuleException($"No teacher-subject assignment found with id '{teacherSubjectAssignmentId}'.");
        }

        if (teacherSubjectAssignment.TeacherId != teacherId)
        {
            throw new ForbiddenAccessException("You can only create assignments for your own teacher-subject assignments.");
        }

        if (!teacherSubjectAssignment.ClassSubject.Class.IsActive)
        {
            throw new BusinessRuleException("This class has been deactivated. No new activity is allowed.");
        }

        if (maxMarks <= 0)
        {
            throw new BusinessRuleException("Max marks must be greater than zero.");
        }

        var now = DateTime.UtcNow;

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            TeacherSubjectAssignmentId = teacherSubjectAssignmentId,
            Title = title,
            Description = description,
            Deadline = deadline,
            MaxMarks = maxMarks,
            AllowResubmission = allowResubmission,
            Status = AssignmentStatus.Draft,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now,
            TeacherSubjectAssignment = teacherSubjectAssignment
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return assignment;
    }

    public async Task<Assignment> UpdateAsync(
        Guid assignmentId,
        string title,
        string? description,
        DateTime deadline,
        decimal maxMarks,
        bool allowResubmission,
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        var assignment = await LoadOwnedAssignmentAsync(assignmentId, teacherId, cancellationToken);

        if (!assignment.TeacherSubjectAssignment.ClassSubject.Class.IsActive)
        {
            throw new BusinessRuleException("This class has been deactivated. No new activity is allowed.");
        }

        if (assignment.Status != AssignmentStatus.Draft)
        {
            throw new BusinessRuleException("Only draft assignments can be edited.");
        }

        if (maxMarks <= 0)
        {
            throw new BusinessRuleException("Max marks must be greater than zero.");
        }

        assignment.Title = title;
        assignment.Description = description;
        assignment.Deadline = deadline;
        assignment.MaxMarks = maxMarks;
        assignment.AllowResubmission = allowResubmission;

        await _context.SaveChangesAsync(cancellationToken);

        return assignment;
    }

    public async Task<Assignment> PublishAsync(Guid assignmentId, Guid teacherId, CancellationToken cancellationToken = default)
    {
        var assignment = await LoadOwnedAssignmentAsync(assignmentId, teacherId, cancellationToken);

        if (!assignment.TeacherSubjectAssignment.ClassSubject.Class.IsActive)
        {
            throw new BusinessRuleException("This class has been deactivated. No new activity is allowed.");
        }

        if (assignment.Status != AssignmentStatus.Draft)
        {
            throw new BusinessRuleException("Only draft assignments can be published.");
        }

        assignment.Status = AssignmentStatus.Published;
        await _context.SaveChangesAsync(cancellationToken);

        return assignment;
    }

    public async Task DeleteAsync(Guid assignmentId, Guid teacherId, CancellationToken cancellationToken = default)
    {
        var assignment = await LoadOwnedAssignmentAsync(assignmentId, teacherId, cancellationToken);

        var hasSubmissions = await _context.Submissions.AnyAsync(s => s.AssignmentId == assignmentId, cancellationToken);
        if (hasSubmissions)
        {
            throw new BusinessRuleException("An assignment that already has submissions cannot be deleted.");
        }

        assignment.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Assignment>> GetForCurrentUserAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default)
    {
        var query = _context.Assignments
            .Include(a => a.TeacherSubjectAssignment).ThenInclude(t => t.Teacher)
            .Include(a => a.TeacherSubjectAssignment).ThenInclude(t => t.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(a => a.TeacherSubjectAssignment).ThenInclude(t => t.ClassSubject).ThenInclude(cs => cs.Subject)
            .Where(a => !a.IsDeleted);

        switch (role)
        {
            case UserRole.Admin:
                return await query.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);

            case UserRole.Teacher:
                return await query
                    .Where(a => a.TeacherSubjectAssignment.TeacherId == userId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync(cancellationToken);

            case UserRole.Student:
                // EF/Npgsql inlines enum literals used directly in a predicate as lowercase SQL text
                // (e.g. 'active'::enrollment_status), which the native Postgres enum rejects since its
                // labels are PascalCase. Routing the value through a local variable forces EF to send it
                // as a parameter instead, which uses the correct enum-to-label mapping.
                var activeStatus = EnrollmentStatus.Active;
                var publishedStatus = AssignmentStatus.Published;

                var enrolledClassIds = await _context.StudentEnrollments
                    .Where(e => e.StudentId == userId && e.Status == activeStatus)
                    .Select(e => e.ClassId)
                    .ToListAsync(cancellationToken);

                return await query
                    .Where(a => a.Status == publishedStatus
                        && enrolledClassIds.Contains(a.TeacherSubjectAssignment.ClassSubject.ClassId))
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync(cancellationToken);

            default:
                return new List<Assignment>();
        }
    }

    public async Task<Assignment> GetByIdAsync(Guid assignmentId, Guid userId, UserRole role, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.Assignments
            .Include(a => a.TeacherSubjectAssignment).ThenInclude(t => t.Teacher)
            .Include(a => a.TeacherSubjectAssignment).ThenInclude(t => t.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(a => a.TeacherSubjectAssignment).ThenInclude(t => t.ClassSubject).ThenInclude(cs => cs.Subject)
            .FirstOrDefaultAsync(a => a.Id == assignmentId && !a.IsDeleted, cancellationToken);

        if (assignment is null)
        {
            throw new BusinessRuleException($"No assignment found with id '{assignmentId}'.");
        }

        switch (role)
        {
            case UserRole.Admin:
                return assignment;

            case UserRole.Teacher:
                if (assignment.TeacherSubjectAssignment.TeacherId != userId)
                {
                    throw new ForbiddenAccessException("You can only view your own assignments.");
                }

                return assignment;

            case UserRole.Student:
                var classId = assignment.TeacherSubjectAssignment.ClassSubject.ClassId;
                var activeStatus = EnrollmentStatus.Active;
                var isEnrolled = await _context.StudentEnrollments.AnyAsync(
                    e => e.StudentId == userId && e.ClassId == classId && e.Status == activeStatus,
                    cancellationToken);

                if (assignment.Status != AssignmentStatus.Published || !isEnrolled)
                {
                    throw new ForbiddenAccessException("You can only view assignments published for a class you're enrolled in.");
                }

                return assignment;

            default:
                throw new ForbiddenAccessException("You are not allowed to view this assignment.");
        }
    }

    private async Task<Assignment> LoadOwnedAssignmentAsync(Guid assignmentId, Guid teacherId, CancellationToken cancellationToken)
    {
        var assignment = await _context.Assignments
            .Include(a => a.TeacherSubjectAssignment).ThenInclude(t => t.Teacher)
            .Include(a => a.TeacherSubjectAssignment).ThenInclude(t => t.ClassSubject).ThenInclude(cs => cs.Class)
            .Include(a => a.TeacherSubjectAssignment).ThenInclude(t => t.ClassSubject).ThenInclude(cs => cs.Subject)
            .FirstOrDefaultAsync(a => a.Id == assignmentId && !a.IsDeleted, cancellationToken);

        if (assignment is null)
        {
            throw new BusinessRuleException($"No assignment found with id '{assignmentId}'.");
        }

        if (assignment.TeacherSubjectAssignment.TeacherId != teacherId)
        {
            throw new ForbiddenAccessException("You can only modify your own assignments.");
        }

        return assignment;
    }
}
