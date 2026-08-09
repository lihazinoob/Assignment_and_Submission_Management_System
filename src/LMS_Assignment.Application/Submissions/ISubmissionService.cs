using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Application.Submissions;

public interface ISubmissionService
{
    Task<Submission> SubmitAsync(
        Guid assignmentId,
        string? answerText,
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task<Submission> UpdateAsync(
        Guid submissionId,
        string? answerText,
        Guid studentId,
        CancellationToken cancellationToken = default);

    Task<Submission> GradeSubmissionAsync(
        Guid submissionId,
        decimal marksObtained,
        string? feedback,
        Guid teacherId,
        CancellationToken cancellationToken = default);

    Task<List<Submission>> GetForCurrentUserAsync(
        Guid userId,
        UserRole role,
        Guid? assignmentId,
        CancellationToken cancellationToken = default);

    Task<Submission> GetByIdAsync(Guid submissionId, Guid userId, UserRole role, CancellationToken cancellationToken = default);
}
