using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Assignment.Infrastructure.Persistence.Configurations;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(s => s.Code).HasColumnName("code").HasMaxLength(30).IsRequired();
        builder.Property(s => s.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(s => s.Code).IsUnique();
    }
}
