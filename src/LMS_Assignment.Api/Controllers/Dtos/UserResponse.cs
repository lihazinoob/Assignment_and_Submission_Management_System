using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Api.Controllers.Dtos;

public record UserResponse(Guid Id, string FullName, string Email, UserRole Role, bool IsActive);
