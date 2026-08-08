namespace LMS_Assignment.Api.Controllers.Dtos;

public record LoginResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);
