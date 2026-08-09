using LMS_Assignment.Domain.Entities;

namespace LMS_Assignment.Application.Classes;

public interface IClassService
{
    Task<Class> CreateAsync(string name, string academicYear, CancellationToken cancellationToken = default);
    Task<List<Class>> GetAllAsync(CancellationToken cancellationToken = default);
}
