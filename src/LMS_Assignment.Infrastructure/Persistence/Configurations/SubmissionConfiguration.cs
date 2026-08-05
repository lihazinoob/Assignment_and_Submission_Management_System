using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Assignment.Infrastructure.Persistence.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("submissions", t => t.HasCheckConstraint("ck_submissions_marks_obtained", "marks_obtained >= 0"));

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.AssignmentId).HasColumnName("assignment_id");
        builder.Property(s => s.StudentId).HasColumnName("student_id");
        builder.Property(s => s.AnswerText).HasColumnName("answer_text");
        builder.Property(s => s.SubmittedAt).HasColumnName("submitted_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        builder.Property(s => s.Status).HasColumnName("status").HasDefaultValue(SubmissionStatus.Submitted);
        builder.Property(s => s.MarksObtained).HasColumnName("marks_obtained").HasColumnType("numeric(6,2)");
        builder.Property(s => s.Feedback).HasColumnName("feedback");
        builder.Property(s => s.GradedBy).HasColumnName("graded_by");
        builder.Property(s => s.GradedAt).HasColumnName("graded_at");

        builder.HasIndex(s => new { s.AssignmentId, s.StudentId }).IsUnique();
        builder.HasIndex(s => s.AssignmentId).HasDatabaseName("idx_submissions_assignment");
        builder.HasIndex(s => s.StudentId).HasDatabaseName("idx_submissions_student");
        builder.HasIndex(s => s.Status).HasDatabaseName("idx_submissions_status");

        builder.HasOne(s => s.Assignment)
            .WithMany(a => a.Submissions)
            .HasForeignKey(s => s.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Student)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Grader)
            .WithMany()
            .HasForeignKey(s => s.GradedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
