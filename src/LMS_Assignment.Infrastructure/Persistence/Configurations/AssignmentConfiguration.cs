using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Assignment.Infrastructure.Persistence.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("assignments", t => t.HasCheckConstraint("ck_assignments_max_marks", "max_marks > 0"));

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.TeacherSubjectAssignmentId).HasColumnName("teacher_subject_assignment_id");
        builder.Property(a => a.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(a => a.Description).HasColumnName("description");
        builder.Property(a => a.Deadline).HasColumnName("deadline");
        builder.Property(a => a.MaxMarks).HasColumnName("max_marks").HasColumnType("numeric(6,2)");
        builder.Property(a => a.AllowResubmission).HasColumnName("allow_resubmission").HasDefaultValue(true);
        builder.Property(a => a.Status).HasColumnName("status").HasDefaultValue(AssignmentStatus.Draft);
        builder.Property(a => a.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(a => a.TeacherSubjectAssignmentId).HasDatabaseName("idx_assignments_tsa");
        builder.HasIndex(a => a.Status).HasDatabaseName("idx_assignments_status");
        builder.HasIndex(a => a.Deadline).HasDatabaseName("idx_assignments_deadline");

        builder.HasOne(a => a.TeacherSubjectAssignment)
            .WithMany(t => t.Assignments)
            .HasForeignKey(a => a.TeacherSubjectAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
