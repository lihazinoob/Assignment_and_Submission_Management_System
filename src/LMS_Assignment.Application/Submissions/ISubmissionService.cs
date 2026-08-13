using LMS_Assignment.Application.Common.Models;
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

    Task<PagedResult<Submission>> GetForCurrentUserAsync(
        Guid userId,
        UserRole role,
        SubmissionFilter filter,
        CancellationToken cancellationToken = default);

    Task<Submission> GetByIdAsync(Guid submissionId, Guid userId, UserRole role, CancellationToken cancellationToken = default);
}
