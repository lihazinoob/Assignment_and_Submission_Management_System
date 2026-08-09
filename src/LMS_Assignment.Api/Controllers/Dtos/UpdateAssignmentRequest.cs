namespace LMS_Assignment.Api.Controllers.Dtos;

public record UpdateAssignmentRequest(
    string Title,
    string? Description,
    DateTime Deadline,
    decimal MaxMarks,
    bool AllowResubmission);
