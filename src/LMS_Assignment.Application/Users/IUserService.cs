using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Application.Users;

public interface IUserService
{
    Task<User> CreateUserAsync(
        string fullName,
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken = default);
}
