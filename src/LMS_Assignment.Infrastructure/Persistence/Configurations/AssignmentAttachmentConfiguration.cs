using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Assignment.Infrastructure.Persistence.Configurations;

public class AssignmentAttachmentConfiguration : IEntityTypeConfiguration<AssignmentAttachment>
{
    public void Configure(EntityTypeBuilder<AssignmentAttachment> builder)
    {
        builder.ToTable("assignment_attachments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.AssignmentId).HasColumnName("assignment_id");
        builder.Property(a => a.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
        builder.Property(a => a.FileUrl).HasColumnName("file_url").HasMaxLength(500).IsRequired();
        builder.Property(a => a.ContentType).HasColumnName("content_type").HasMaxLength(100);
        builder.Property(a => a.FileSizeBytes).HasColumnName("file_size_bytes");
        builder.Property(a => a.UploadedAt).HasColumnName("uploaded_at");

        builder.HasIndex(a => a.AssignmentId).HasDatabaseName("idx_assignment_attachments_assignment");

        builder.HasOne(a => a.Assignment)
            .WithMany(x => x.Attachments)
            .HasForeignKey(a => a.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
