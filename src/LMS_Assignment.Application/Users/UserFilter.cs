using LMS_Assignment.Application.Common.Models;
using LMS_Assignment.Domain.Enums;

namespace LMS_Assignment.Application.Users;

public class UserFilter : PaginationQuery
{
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
}
