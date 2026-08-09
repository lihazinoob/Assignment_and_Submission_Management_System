namespace LMS_Assignment.Api.Controllers.Dtos;

public record CreateTeacherSubjectAssignmentRequest(Guid TeacherId, Guid ClassSubjectId);
