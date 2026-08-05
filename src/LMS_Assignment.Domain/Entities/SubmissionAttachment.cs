namespace LMS_Assignment.Domain.Entities;

public class SubmissionAttachment
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Submission Submission { get; set; } = null!;
}
