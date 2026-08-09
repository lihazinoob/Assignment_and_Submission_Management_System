using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Api.Controllers.Dtos;

public record AssignmentResponse(
    Guid Id,
    Guid TeacherSubjectAssignmentId,
    string TeacherName,
    string ClassName,
    string SubjectName,
    string Title,
    string? Description,
    DateTime Deadline,
    decimal MaxMarks,
    bool AllowResubmission,
    AssignmentStatus Status);
