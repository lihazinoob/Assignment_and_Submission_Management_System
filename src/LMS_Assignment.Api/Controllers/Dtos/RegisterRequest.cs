using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Api.Controllers.Dtos;

public record RegisterRequest(string FullName, string Email, string Password, UserRole Role);
