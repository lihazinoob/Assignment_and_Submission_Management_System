using LMS_Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMS_Assignment.Infrastructure.Persistence.Configurations;

public class SubmissionStatusHistoryConfiguration : IEntityTypeConfiguration<SubmissionStatusHistory>
{
    public void Configure(EntityTypeBuilder<SubmissionStatusHistory> builder)
    {
        builder.ToTable("submission_status_history");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id");

        builder.Property(h => h.SubmissionId).HasColumnName("submission_id");
        builder.Property(h => h.OldStatus).HasColumnName("old_status");
        builder.Property(h => h.NewStatus).HasColumnName("new_status").IsRequired();
        builder.Property(h => h.ChangedBy).HasColumnName("changed_by");
        builder.Property(h => h.ChangedAt).HasColumnName("changed_at");
        builder.Property(h => h.Remarks).HasColumnName("remarks").HasMaxLength(500);

        builder.HasIndex(h => h.SubmissionId).HasDatabaseName("idx_status_history_submission");

        builder.HasOne(h => h.Submission)
            .WithMany(s => s.StatusHistory)
            .HasForeignKey(h => h.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.ChangedByUser)
            .WithMany()
            .HasForeignKey(h => h.ChangedBy)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
