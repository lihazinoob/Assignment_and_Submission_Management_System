using System.Text;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Enums;
using LMS_Assignment.Infrastructure.Persistence;
using LMS_Assignment.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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
        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dataSource, npgsqlOptions =>
        {
            npgsqlOptions.MapEnum<UserRole>("user_role");
            npgsqlOptions.MapEnum<EnrollmentStatus>("enrollment_status");
            npgsqlOptions.MapEnum<AssignmentStatus>("assignment_status");
            npgsqlOptions.MapEnum<SubmissionStatus>("submission_status");
        }));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        var jwtSection = configuration.GetSection("Jwt");
        var jwtKey = jwtSection["Key"]
            ?? throw new InvalidOperationException("Jwt:Key was not found in configuration.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSection["Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        return services;
    }
}
