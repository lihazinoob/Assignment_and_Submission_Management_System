using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Enums;
using LMS_Assignment.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.NameTranslation;

namespace LMS_Assignment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<UserRole>("user_role", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<EnrollmentStatus>("enrollment_status", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<AssignmentStatus>("assignment_status", new NpgsqlNullNameTranslator());
        dataSourceBuilder.MapEnum<SubmissionStatus>("submission_status", new NpgsqlNullNameTranslator());
        var dataSource = dataSourceBuilder.Build();

        services.AddSingleton(dataSource);
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dataSource));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }
}
