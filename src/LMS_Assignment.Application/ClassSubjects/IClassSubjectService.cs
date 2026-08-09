using LMS_Assignment.Domain.Entities;

namespace LMS_Assignment.Application.ClassSubjects;

public interface IClassSubjectService
{
    Task<ClassSubject> CreateAsync(Guid classId, Guid subjectId, CancellationToken cancellationToken = default);
    Task<List<ClassSubject>> GetAllAsync(CancellationToken cancellationToken = default);
}
