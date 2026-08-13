using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Extensions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Application.Common.Models;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Application.Users;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<User> CreateUserAsync(
        string fullName,
        string email,
        string password,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var emailTaken = await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        if (emailTaken)
        {
            throw new BusinessRuleException($"A user with email '{email}' already exists.");
        }

        var now = DateTime.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            PasswordHash = _passwordHasher.Hash(password),
            Role = role,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<PagedResult<User>> GetUsersAsync(UserFilter filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        if (filter.Role.HasValue)
        {
            var requestedRole = filter.Role.Value;
            query = query.Where(u => u.Role == requestedRole);
        }

        if (filter.IsActive.HasValue)
        {
            var isActiveFilter = filter.IsActive.Value;
            query = query.Where(u => u.IsActive == isActiveFilter);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchTerm = filter.Search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(searchTerm) || u.Email.ToLower().Contains(searchTerm));
        }

        query = query.OrderBy(u => u.FullName);

        return await query.ToPagedResultAsync(filter.Page, filter.PageSize, cancellationToken);
    }

    public async Task<User> DeactivateUserAsync(
        Guid userId,
        Guid currentAdminId,
        CancellationToken cancellationToken = default)
    {
        if (userId == currentAdminId)
        {
            throw new BusinessRuleException("You cannot deactivate your own account.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            throw new BusinessRuleException("User not found.");
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }

    public async Task<User> ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            throw new BusinessRuleException("User not found.");
        }

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }
}
