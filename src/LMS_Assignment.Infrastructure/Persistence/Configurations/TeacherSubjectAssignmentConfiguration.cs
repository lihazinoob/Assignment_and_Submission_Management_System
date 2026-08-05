using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Assignment.Infrastructure.Persistence.Configurations;

public class TeacherSubjectAssignmentConfiguration : IEntityTypeConfiguration<TeacherSubjectAssignment>
{
    public void Configure(EntityTypeBuilder<TeacherSubjectAssignment> builder)
    {
        builder.ToTable("teacher_subject_assignments");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.TeacherId).HasColumnName("teacher_id");
        builder.Property(t => t.ClassSubjectId).HasColumnName("class_subject_id");
        builder.Property(t => t.AssignedBy).HasColumnName("assigned_by");
        builder.Property(t => t.AssignedAt).HasColumnName("assigned_at");

        builder.HasIndex(t => new { t.TeacherId, t.ClassSubjectId }).IsUnique();
        builder.HasIndex(t => t.TeacherId).HasDatabaseName("idx_tsa_teacher");
        builder.HasIndex(t => t.ClassSubjectId).HasDatabaseName("idx_tsa_class_subject");

        builder.HasOne(t => t.Teacher)
            .WithMany(u => u.TeacherSubjectAssignments)
            .HasForeignKey(t => t.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ClassSubject)
            .WithMany(cs => cs.TeacherSubjectAssignments)
            .HasForeignKey(t => t.ClassSubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.AssignedByUser)
            .WithMany()
            .HasForeignKey(t => t.AssignedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
