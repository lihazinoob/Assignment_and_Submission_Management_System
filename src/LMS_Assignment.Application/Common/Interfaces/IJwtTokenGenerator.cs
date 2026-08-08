using LMS_Assignment.Domain.Entities;

namespace LMS_Assignment.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    TimeSpan AccessTokenLifetime { get; }

    TimeSpan RefreshTokenLifetime { get; }

    string GenerateAccessToken(User user);

    string GenerateRefreshToken();
}
