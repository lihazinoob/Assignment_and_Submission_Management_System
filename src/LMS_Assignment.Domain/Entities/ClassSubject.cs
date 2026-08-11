namespace LMS_Assignment.Domain.Entities;

public class ClassSubject
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Class Class { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ICollection<TeacherSubjectAssignment> TeacherSubjectAssignments { get; set; } = new List<TeacherSubjectAssignment>();
}
