using LMS_Assignment.Domain.Common;
using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Domain.Entities;

public class Submission : IHasUpdatedAt
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public string? AnswerText { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;
    public decimal? MarksObtained { get; set; }
    public string? Feedback { get; set; }
    public Guid? GradedBy { get; set; }
    public DateTime? GradedAt { get; set; }

    // Navigation properties
    public Assignment Assignment { get; set; } = null!;
    public User Student { get; set; } = null!;
    public User? Grader { get; set; }
    public ICollection<SubmissionAttachment> Attachments { get; set; } = new List<SubmissionAttachment>();
    public ICollection<SubmissionStatusHistory> StatusHistory { get; set; } = new List<SubmissionStatusHistory>();
}
