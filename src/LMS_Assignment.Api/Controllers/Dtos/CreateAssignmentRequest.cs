namespace LMS_Assignment.Api.Controllers.Dtos;

public record CreateAssignmentRequest(
    Guid TeacherSubjectAssignmentId,
    string Title,
    string? Description,
    DateTime Deadline,
    decimal MaxMarks,
    bool AllowResubmission);
