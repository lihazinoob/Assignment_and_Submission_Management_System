using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Class> Classes { get; }
    DbSet<Subject> Subjects { get; }
    DbSet<ClassSubject> ClassSubjects { get; }
    DbSet<TeacherSubjectAssignment> TeacherSubjectAssignments { get; }
    DbSet<StudentEnrollment> StudentEnrollments { get; }
    DbSet<Assignment> Assignments { get; }
    DbSet<AssignmentAttachment> AssignmentAttachments { get; }
    DbSet<Submission> Submissions { get; }
    DbSet<SubmissionAttachment> SubmissionAttachments { get; }
    DbSet<SubmissionStatusHistory> SubmissionStatusHistories { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<AppSetting> AppSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
