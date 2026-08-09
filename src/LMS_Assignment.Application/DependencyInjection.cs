using LMS_Assignment.Application.Auth;
using LMS_Assignment.Application.Submissions;
using LMS_Assignment.Application.TeacherSubjectAssignments;
using LMS_Assignment.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace LMS_Assignment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITeacherSubjectAssignmentService, TeacherSubjectAssignmentService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
