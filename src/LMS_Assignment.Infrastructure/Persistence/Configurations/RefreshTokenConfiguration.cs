using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Assignment.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.UserId).HasColumnName("user_id");
        builder.Property(r => r.TokenHash).HasColumnName("token_hash").HasMaxLength(255).IsRequired();
        builder.Property(r => r.ExpiresAt).HasColumnName("expires_at");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.RevokedAt).HasColumnName("revoked_at");
        builder.Property(r => r.CreatedByIp).HasColumnName("created_by_ip").HasMaxLength(50);

        builder.HasIndex(r => r.TokenHash).IsUnique();
        builder.HasIndex(r => r.UserId).HasDatabaseName("idx_refresh_tokens_user");

        builder.HasOne(r => r.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
