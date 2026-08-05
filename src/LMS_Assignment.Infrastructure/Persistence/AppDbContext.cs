using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS_Assignment.Infrastructure.Persistence;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ClassSubject> ClassSubjects => Set<ClassSubject>();
    public DbSet<TeacherSubjectAssignment> TeacherSubjectAssignments => Set<TeacherSubjectAssignment>();
    public DbSet<StudentEnrollment> StudentEnrollments => Set<StudentEnrollment>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentAttachment> AssignmentAttachments => Set<AssignmentAttachment>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<SubmissionAttachment> SubmissionAttachments => Set<SubmissionAttachment>();
    public DbSet<SubmissionStatusHistory> SubmissionStatusHistories => Set<SubmissionStatusHistory>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<UserRole>("user_role");
        modelBuilder.HasPostgresEnum<EnrollmentStatus>("enrollment_status");
        modelBuilder.HasPostgresEnum<AssignmentStatus>("assignment_status");
        modelBuilder.HasPostgresEnum<SubmissionStatus>("submission_status");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

}
