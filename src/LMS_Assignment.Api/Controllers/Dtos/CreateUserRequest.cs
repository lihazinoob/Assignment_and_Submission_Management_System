using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Api.Controllers.Dtos;

public record CreateUserRequest(string FullName, string Email, string Password, UserRole Role);
