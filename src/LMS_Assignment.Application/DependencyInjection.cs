using LMS_Assignment.Application.Submissions;
using LMS_Assignment.Application.TeacherSubjectAssignments;
using Microsoft.Extensions.DependencyInjection;

namespace LMS_Assignment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITeacherSubjectAssignmentService, TeacherSubjectAssignmentService>();
        services.AddScoped<ISubmissionService, SubmissionService>();

        return services;
    }
}
