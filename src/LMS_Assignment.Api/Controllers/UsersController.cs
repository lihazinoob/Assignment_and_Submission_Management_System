using LMS_Assignment.Api.Controllers.Dtos;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Application.Users;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Assignment.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserResponse>>> GetAll(
        [FromQuery] UserRole? role,
        [FromQuery] bool? isActive,
        [FromQuery] string? search,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var filter = new UserFilter
        {
            Role = role,
            IsActive = isActive,
            Search = search,
            Page = page,
            PageSize = pageSize
        };

        var result = await _userService.GetUsersAsync(filter, cancellationToken);

        return Ok(new PagedResponse<UserResponse>(
            result.Items.Select(ToResponse).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize));
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<UserResponse>> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.DeactivateUserAsync(
            id, _currentUserService.UserId!.Value, cancellationToken);

        return Ok(ToResponse(user));
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<UserResponse>> Activate(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.ActivateUserAsync(id, cancellationToken);

        return Ok(ToResponse(user));
    }

    private static UserResponse ToResponse(User user) =>
        new(user.Id, user.FullName, user.Email, user.Role, user.IsActive);
}
