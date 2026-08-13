using LMS_Assignment.Application.Common.Models;
using LMS_Assignment.Domain.Entities;

namespace LMS_Assignment.Application.StudentEnrollments;

public interface IStudentEnrollmentService
{
    Task<StudentEnrollment> EnrollAsync(
        Guid studentId,
        Guid classId,
        string? rollNumber,
        CancellationToken cancellationToken = default);

    Task<PagedResult<StudentEnrollment>> GetAllAsync(PaginationQuery pagination, CancellationToken cancellationToken = default);
}
