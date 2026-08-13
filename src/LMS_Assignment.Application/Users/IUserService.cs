using LMS_Assignment.Application.Common.Models;
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

    Task<PagedResult<User>> GetUsersAsync(
        UserFilter filter,
        CancellationToken cancellationToken = default);

    Task<User> DeactivateUserAsync(
        Guid userId,
        Guid currentAdminId,
        CancellationToken cancellationToken = default);

    Task<User> ActivateUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
