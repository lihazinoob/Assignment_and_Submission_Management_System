namespace LMS_Assignment.Domain.Entities;

public class TeacherSubjectAssignment
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid ClassSubjectId { get; set; }
    public Guid? AssignedBy { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Teacher { get; set; } = null!;
    public ClassSubject ClassSubject { get; set; } = null!;
    public User? AssignedByUser { get; set; }
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
