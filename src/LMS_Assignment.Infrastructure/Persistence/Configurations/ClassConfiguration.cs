using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Assignment.Infrastructure.Persistence.Configurations;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("classes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(c => c.AcademicYear).HasColumnName("academic_year").HasMaxLength(20).IsRequired();
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(c => new { c.Name, c.AcademicYear }).IsUnique();
    }
}
