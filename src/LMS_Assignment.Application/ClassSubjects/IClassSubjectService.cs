using LMS_Assignment.Application.Common.Models;
using LMS_Assignment.Domain.Entities;

namespace LMS_Assignment.Application.ClassSubjects;

public interface IClassSubjectService
{
    Task<ClassSubject> CreateAsync(Guid classId, Guid subjectId, CancellationToken cancellationToken = default);
    Task<PagedResult<ClassSubject>> GetAllAsync(PaginationQuery pagination, CancellationToken cancellationToken = default);
    Task<ClassSubject> DeactivateAsync(Guid classSubjectId, CancellationToken cancellationToken = default);
    Task<ClassSubject> ActivateAsync(Guid classSubjectId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid classSubjectId, CancellationToken cancellationToken = default);
}
