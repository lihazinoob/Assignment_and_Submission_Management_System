namespace LMS_Assignment.Domain.Entities;

public class AssignmentAttachment
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Assignment Assignment { get; set; } = null!;
}
