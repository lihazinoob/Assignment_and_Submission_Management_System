using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Interfaces;
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

    public async Task<List<User>> GetUsersAsync(UserRole? role, CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        if (role.HasValue)
        {
            var requestedRole = role.Value;
            query = query.Where(u => u.Role == requestedRole);
        }

        return await query.OrderBy(u => u.FullName).ToListAsync(cancellationToken);
    }
}
