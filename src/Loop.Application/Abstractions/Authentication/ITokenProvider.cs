using Loop.Domain.Users;

namespace Loop.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string CreateAccessToken(User user);
    (string RefreshToken, DateTime ExpiresAtUtc) CreateRefreshToken();
}

