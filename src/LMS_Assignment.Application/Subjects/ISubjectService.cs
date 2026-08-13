using LMS_Assignment.Application.Common.Models;
using LMS_Assignment.Domain.Entities;

namespace LMS_Assignment.Application.Subjects;

public interface ISubjectService
{
    Task<Subject> CreateAsync(string name, string code, CancellationToken cancellationToken = default);
    Task<PagedResult<Subject>> GetAllAsync(PaginationQuery pagination, CancellationToken cancellationToken = default);
}
