using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Assignment.Infrastructure.Persistence.Configurations;

public class StudentEnrollmentConfiguration : IEntityTypeConfiguration<StudentEnrollment>
{
    public void Configure(EntityTypeBuilder<StudentEnrollment> builder)
    {
        builder.ToTable("student_enrollments");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.StudentId).HasColumnName("student_id");
        builder.Property(e => e.ClassId).HasColumnName("class_id");
        builder.Property(e => e.RollNumber).HasColumnName("roll_number").HasMaxLength(30);
        builder.Property(e => e.Status).HasColumnName("status").HasColumnType("enrollment_status").HasDefaultValueSql("'Active'::enrollment_status");
        builder.Property(e => e.EnrolledAt).HasColumnName("enrolled_at");

        builder.HasIndex(e => new { e.StudentId, e.ClassId }).IsUnique();
        builder.HasIndex(e => e.StudentId).HasDatabaseName("idx_enrollments_student");
        builder.HasIndex(e => e.ClassId).HasDatabaseName("idx_enrollments_class");

        builder.HasOne(e => e.Student)
            .WithMany(u => u.StudentEnrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Class)
            .WithMany(c => c.StudentEnrollments)
            .HasForeignKey(e => e.ClassId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
