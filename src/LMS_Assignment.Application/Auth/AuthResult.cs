namespace LMS_Assignment.Application.Auth;

public record AuthResult(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);
