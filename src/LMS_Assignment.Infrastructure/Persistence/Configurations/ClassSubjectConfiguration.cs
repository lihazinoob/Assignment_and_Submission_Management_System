using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Assignment.Infrastructure.Persistence.Configurations;

public class ClassSubjectConfiguration : IEntityTypeConfiguration<ClassSubject>
{
    public void Configure(EntityTypeBuilder<ClassSubject> builder)
    {
        builder.ToTable("class_subjects");

        builder.HasKey(cs => cs.Id);
        builder.Property(cs => cs.Id).HasColumnName("id");

        builder.Property(cs => cs.ClassId).HasColumnName("class_id");
        builder.Property(cs => cs.SubjectId).HasColumnName("subject_id");
        builder.Property(cs => cs.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(cs => cs.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(cs => new { cs.ClassId, cs.SubjectId }).IsUnique();
        builder.HasIndex(cs => cs.ClassId).HasDatabaseName("idx_class_subjects_class");
        builder.HasIndex(cs => cs.SubjectId).HasDatabaseName("idx_class_subjects_subject");

        builder.HasOne(cs => cs.Class)
            .WithMany(c => c.ClassSubjects)
            .HasForeignKey(cs => cs.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cs => cs.Subject)
            .WithMany(s => s.ClassSubjects)
            .HasForeignKey(cs => cs.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
