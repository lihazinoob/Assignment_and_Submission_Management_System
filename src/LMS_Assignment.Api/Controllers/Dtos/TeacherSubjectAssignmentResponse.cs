namespace LMS_Assignment.Api.Controllers.Dtos;

public record TeacherSubjectAssignmentResponse(
    Guid Id,
    Guid TeacherId,
    string TeacherName,
    Guid ClassSubjectId,
    string ClassName,
    string SubjectName,
    Guid? AssignedBy,
    DateTime AssignedAt);
