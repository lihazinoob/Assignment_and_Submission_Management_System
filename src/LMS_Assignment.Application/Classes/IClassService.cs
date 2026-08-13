using LMS_Assignment.Application.Common.Models;
using LMS_Assignment.Domain.Entities;

namespace LMS_Assignment.Application.Classes;

public interface IClassService
{
    Task<Class> CreateAsync(string name, string academicYear, CancellationToken cancellationToken = default);
    Task<PagedResult<Class>> GetAllAsync(PaginationQuery pagination, CancellationToken cancellationToken = default);

    Task<Class> UpdateAsync(
        Guid classId,
        string name,
        string academicYear,
        CancellationToken cancellationToken = default);

    Task<Class> DeactivateAsync(Guid classId, CancellationToken cancellationToken = default);
    Task<Class> ActivateAsync(Guid classId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid classId, CancellationToken cancellationToken = default);
}
