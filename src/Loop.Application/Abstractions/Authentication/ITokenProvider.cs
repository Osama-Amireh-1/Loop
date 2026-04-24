using Loop.Domain.Shops;
using Loop.Domain.Users;

namespace Loop.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string CreateAccessToken(User user);
    string CreateAccessToken(ShopAdmin shopAdmin);
    (string RefreshToken, DateTime ExpiresAtUtc) CreateRefreshToken();
}

