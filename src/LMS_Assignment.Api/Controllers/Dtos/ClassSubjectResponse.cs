namespace LMS_Assignment.Api.Controllers.Dtos;

public record ClassSubjectResponse(Guid Id, Guid ClassId, string ClassName, Guid SubjectId, string SubjectName);
