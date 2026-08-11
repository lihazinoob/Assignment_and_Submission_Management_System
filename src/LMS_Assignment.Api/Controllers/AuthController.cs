using LMS_Assignment.Api.Controllers.Dtos;
using LMS_Assignment.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_Assignment.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(request.Email, request.Password, ipAddress, cancellationToken);

        return Ok(new LoginResponse(result.AccessToken, result.RefreshToken, result.AccessTokenExpiresAt));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress, cancellationToken);

        return Ok(new LoginResponse(result.AccessToken, result.RefreshToken, result.AccessTokenExpiresAt));
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RegisterAsync(
            request.FullName, request.Email, request.Password, request.Role, ipAddress, cancellationToken);

        return Ok(new LoginResponse(result.AccessToken, result.RefreshToken, result.AccessTokenExpiresAt));
    }
}
