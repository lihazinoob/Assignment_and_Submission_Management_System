using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Domain.Entities;

public class SubmissionStatusHistory
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public SubmissionStatus? OldStatus { get; set; }
    public SubmissionStatus NewStatus { get; set; }
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? Remarks { get; set; }

    // Navigation properties
    public Submission Submission { get; set; } = null!;
    public User? ChangedByUser { get; set; }
}
