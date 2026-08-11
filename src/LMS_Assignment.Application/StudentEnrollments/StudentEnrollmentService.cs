using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Application.StudentEnrollments;

public class StudentEnrollmentService : IStudentEnrollmentService
{
    private readonly IApplicationDbContext _context;

    public StudentEnrollmentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentEnrollment> EnrollAsync(
        Guid studentId,
        Guid classId,
        string? rollNumber,
        CancellationToken cancellationToken = default)
    {
        var student = await _context.Users.FirstOrDefaultAsync(u => u.Id == studentId, cancellationToken);
        if (student is null || student.Role != UserRole.Student)
        {
            throw new BusinessRuleException($"User '{studentId}' does not belong to a Student account.");
        }

        var @class = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);
        if (@class is null)
        {
            throw new BusinessRuleException($"No class found with id '{classId}'.");
        }

        if (!@class.IsActive)
        {
            throw new BusinessRuleException("This class has been deactivated. No new activity is allowed.");
        }

        var alreadyEnrolled = await _context.StudentEnrollments.AnyAsync(
            e => e.StudentId == studentId && e.ClassId == classId,
            cancellationToken);

        if (alreadyEnrolled)
        {
            throw new BusinessRuleException("This student is already enrolled in this class.");
        }

        var enrollment = new StudentEnrollment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            ClassId = classId,
            RollNumber = rollNumber,
            Status = EnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow,
            Student = student,
            Class = @class
        };

        _context.StudentEnrollments.Add(enrollment);
        await _context.SaveChangesAsync(cancellationToken);

        return enrollment;
    }

    public async Task<List<StudentEnrollment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.StudentEnrollments
            .Include(e => e.Student)
            .Include(e => e.Class)
            .OrderBy(e => e.EnrolledAt)
            .ToListAsync(cancellationToken);
    }
}
