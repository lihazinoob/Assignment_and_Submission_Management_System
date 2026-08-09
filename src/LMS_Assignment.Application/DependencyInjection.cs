using LMS_Assignment.Application.Assignments;
using LMS_Assignment.Application.Auth;
using LMS_Assignment.Application.Classes;
using LMS_Assignment.Application.ClassSubjects;
using LMS_Assignment.Application.StudentEnrollments;
using LMS_Assignment.Application.Submissions;
using LMS_Assignment.Application.Subjects;
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
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<IClassSubjectService, ClassSubjectService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IStudentEnrollmentService, StudentEnrollmentService>();

        return services;
    }
}
