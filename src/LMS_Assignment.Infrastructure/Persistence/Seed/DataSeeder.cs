using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Infrastructure.Persistence.Seed;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;

        context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FullName = "Demo Admin",
            Email = "admin@lms.demo",
            PasswordHash = passwordHasher.Hash("Admin@123"),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        await context.SaveChangesAsync();
    }
}
