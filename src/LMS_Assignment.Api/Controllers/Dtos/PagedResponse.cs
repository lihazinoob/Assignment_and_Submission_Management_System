namespace LMS_Assignment.Api.Controllers.Dtos;

public record PagedResponse<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
