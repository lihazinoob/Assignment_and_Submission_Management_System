using LMS_Assignment.Domain.Entities;

namespace LMS_Assignment.Application.TeacherSubjectAssignments;

public interface ITeacherSubjectAssignmentService
{
    Task<TeacherSubjectAssignment> AssignTeacherAsync(
        Guid teacherId,
        Guid classSubjectId,
        Guid? assignedBy,
        CancellationToken cancellationToken = default);
}
