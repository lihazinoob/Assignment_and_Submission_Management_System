using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Domain.Entities;

public class Assignment
{
    public Guid Id { get; set; }
    public Guid TeacherSubjectAssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Deadline { get; set; }
    public decimal MaxMarks { get; set; }
    public bool AllowResubmission { get; set; } = true;
    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public TeacherSubjectAssignment TeacherSubjectAssignment { get; set; } = null!;
    public ICollection<AssignmentAttachment> Attachments { get; set; } = new List<AssignmentAttachment>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
