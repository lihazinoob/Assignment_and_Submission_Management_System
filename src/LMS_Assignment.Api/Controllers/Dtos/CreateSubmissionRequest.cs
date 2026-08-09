namespace LMS_Assignment.Api.Controllers.Dtos;

public record CreateSubmissionRequest(Guid AssignmentId, string? AnswerText);
