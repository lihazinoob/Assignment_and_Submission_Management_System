namespace LMS_Assignment.Api.Controllers.Dtos;

public record CreateStudentEnrollmentRequest(Guid StudentId, Guid ClassId, string? RollNumber);
