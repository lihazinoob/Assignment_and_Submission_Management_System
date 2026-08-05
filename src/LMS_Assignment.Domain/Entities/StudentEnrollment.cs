using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Domain.Entities;

public class StudentEnrollment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public string? RollNumber { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User Student { get; set; } = null!;
    public Class Class { get; set; } = null!;
}
