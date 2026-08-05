using LMS_Assignment.Domain.Enums;
using LMS_Assignment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace LMS_Assignment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<UserRole>("user_role");
        dataSourceBuilder.MapEnum<EnrollmentStatus>("enrollment_status");
        dataSourceBuilder.MapEnum<AssignmentStatus>("assignment_status");
        dataSourceBuilder.MapEnum<SubmissionStatus>("submission_status");
        var dataSource = dataSourceBuilder.Build();

        services.AddSingleton(dataSource);
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dataSource));

        return services;
    }
}
