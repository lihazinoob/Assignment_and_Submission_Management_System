using LMS_Assignment.Domain.Entities;

namespace LMS_Assignment.Application.Submissions;

public interface ISubmissionService
{
    Task<Submission> GradeSubmissionAsync(
        Guid submissionId,
        decimal marksObtained,
        string? feedback,
        Guid gradedBy,
        CancellationToken cancellationToken = default);
}
