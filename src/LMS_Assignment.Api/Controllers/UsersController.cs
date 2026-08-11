using LMS_Assignment.Api.Controllers.Dtos;
using LMS_Assignment.Application.Users;
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

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll([FromQuery] UserRole? role, CancellationToken cancellationToken)
    {
        var users = await _userService.GetUsersAsync(role, cancellationToken);

        var response = users
            .Select(u => new UserResponse(u.Id, u.FullName, u.Email, u.Role, u.IsActive))
            .ToList();

        return Ok(response);
    }
}
