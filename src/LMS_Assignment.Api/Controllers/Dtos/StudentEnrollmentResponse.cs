using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Api.Controllers.Dtos;

public record StudentEnrollmentResponse(
    Guid Id,
    Guid StudentId,
    string StudentName,
    Guid ClassId,
    string ClassName,
    string? RollNumber,
    EnrollmentStatus Status,
    DateTime EnrolledAt);
