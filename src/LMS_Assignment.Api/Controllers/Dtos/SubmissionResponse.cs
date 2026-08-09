using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Api.Controllers.Dtos;

public record SubmissionResponse(
    Guid Id,
    Guid AssignmentId,
    string AssignmentTitle,
    Guid StudentId,
    string StudentName,
    string? AnswerText,
    DateTime SubmittedAt,
    SubmissionStatus Status,
    decimal? MarksObtained,
    string? Feedback,
    Guid? GradedBy,
    DateTime? GradedAt);
