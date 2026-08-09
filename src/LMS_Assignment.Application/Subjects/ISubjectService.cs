using LMS_Assignment.Domain.Entities;

namespace LMS_Assignment.Application.Subjects;

public interface ISubjectService
{
    Task<Subject> CreateAsync(string name, string code, CancellationToken cancellationToken = default);
    Task<List<Subject>> GetAllAsync(CancellationToken cancellationToken = default);
}
