using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Assignment.Infrastructure.Persistence.Configurations;

public class AppSettingConfiguration : IEntityTypeConfiguration<AppSetting>
{
    public void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        builder.ToTable("app_settings");

        builder.HasKey(s => s.Key);
        builder.Property(s => s.Key).HasColumnName("key").HasMaxLength(100);

        builder.Property(s => s.Value).HasColumnName("value").IsRequired();
        builder.Property(s => s.UpdatedBy).HasColumnName("updated_by");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(s => s.UpdatedByUser)
            .WithMany()
            .HasForeignKey(s => s.UpdatedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
