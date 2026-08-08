using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Tests;

public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options) : base(options)
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

    public static TestApplicationDbContext CreateNew()
    {
        var options = new DbContextOptionsBuilder<TestApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestApplicationDbContext(options);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // These two entities have two FKs pointing at User, which EF's conventions
        // can't disambiguate on their own — spell out the pairing explicitly.
        modelBuilder.Entity<Submission>(b =>
        {
            b.HasOne(s => s.Student).WithMany(u => u.Submissions).HasForeignKey(s => s.StudentId);
            b.HasOne(s => s.Grader).WithMany().HasForeignKey(s => s.GradedBy);
        });

        modelBuilder.Entity<TeacherSubjectAssignment>(b =>
        {
            b.HasOne(t => t.Teacher).WithMany(u => u.TeacherSubjectAssignments).HasForeignKey(t => t.TeacherId);
            b.HasOne(t => t.AssignedByUser).WithMany().HasForeignKey(t => t.AssignedBy);
        });

        modelBuilder.Entity<AppSetting>().HasKey(s => s.Key);

        base.OnModelCreating(modelBuilder);
    }
}
