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

        var assignment = new TeacherSubjectAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = teacherId,
            ClassSubjectId = classSubjectId,
            AssignedBy = assignedBy,
            AssignedAt = DateTime.UtcNow
        };

        _context.TeacherSubjectAssignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return assignment;
    }
}
