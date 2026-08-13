using LMS_Assignment.Application.Common.Models;
using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Application.Assignments;

public class AssignmentFilter : PaginationQuery
{
    public AssignmentStatus? Status { get; set; }
    public Guid? ClassSubjectId { get; set; }
    public string? Search { get; set; }
}
