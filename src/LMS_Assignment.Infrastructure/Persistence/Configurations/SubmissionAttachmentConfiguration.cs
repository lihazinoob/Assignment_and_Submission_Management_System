using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Assignment.Infrastructure.Persistence.Configurations;

public class SubmissionAttachmentConfiguration : IEntityTypeConfiguration<SubmissionAttachment>
{
    public void Configure(EntityTypeBuilder<SubmissionAttachment> builder)
    {
        builder.ToTable("submission_attachments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.SubmissionId).HasColumnName("submission_id");
        builder.Property(a => a.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
        builder.Property(a => a.FileUrl).HasColumnName("file_url").HasMaxLength(500).IsRequired();
        builder.Property(a => a.ContentType).HasColumnName("content_type").HasMaxLength(100);
        builder.Property(a => a.FileSizeBytes).HasColumnName("file_size_bytes");
        builder.Property(a => a.UploadedAt).HasColumnName("uploaded_at");

        builder.HasIndex(a => a.SubmissionId).HasDatabaseName("idx_submission_attachments_submission");

        builder.HasOne(a => a.Submission)
            .WithMany(s => s.Attachments)
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
