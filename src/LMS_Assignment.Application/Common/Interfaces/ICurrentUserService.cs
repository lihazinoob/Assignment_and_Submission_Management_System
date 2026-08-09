using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    UserRole? Role { get; }
}
