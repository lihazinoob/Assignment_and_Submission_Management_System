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

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _userService.CreateUserAsync(
            request.FullName,
            request.Email,
            request.Password,
            request.Role,
            cancellationToken);

        var response = new UserResponse(user.Id, user.FullName, user.Email, user.Role, user.IsActive);

        return Created($"/api/users/{user.Id}", response);
    }
}
