using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Application.TeacherSubjectAssignments;

public interface ITeacherSubjectAssignmentService
{
    Task<TeacherSubjectAssignment> AssignTeacherAsync(
        Guid teacherId,
        Guid classSubjectId,
        Guid? assignedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin sees every assignment; a Teacher sees only their own — a Teacher token has no
    /// other way to discover the teacherSubjectAssignmentId needed to create an Assignment.
    /// </summary>
    Task<List<TeacherSubjectAssignment>> GetForCurrentUserAsync(
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default);
}
