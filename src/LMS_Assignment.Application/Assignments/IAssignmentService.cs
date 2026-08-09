using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Application.Assignments;

public interface IAssignmentService
{
    Task<Assignment> CreateAsync(
        Guid teacherSubjectAssignmentId,
        string title,
        string? description,
        DateTime deadline,
        decimal maxMarks,
        bool allowResubmission,
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<Assignment> UpdateAsync(
        Guid assignmentId,
        string title,
        string? description,
        DateTime deadline,
        decimal maxMarks,
        bool allowResubmission,
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<Assignment> PublishAsync(Guid assignmentId, Guid teacherId, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid assignmentId, Guid teacherId, CancellationToken cancellationToken = default);

    Task<List<Assignment>> GetForCurrentUserAsync(Guid userId, UserRole role, CancellationToken cancellationToken = default);

    Task<Assignment> GetByIdAsync(Guid assignmentId, Guid userId, UserRole role, CancellationToken cancellationToken = default);
}
