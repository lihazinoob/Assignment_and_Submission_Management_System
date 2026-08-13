using LMS_Assignment.Application.Common.Models;
using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Application.Submissions;

public class SubmissionFilter : PaginationQuery
{
    public Guid? AssignmentId { get; set; }
    public SubmissionStatus? Status { get; set; }
}
