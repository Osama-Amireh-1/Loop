using Loop.Domain.Specifications;

namespace Loop.Domain.Users.Specifications;

public sealed class UserSessionByRefreshTokenHashSpecification : Specification<UserSession>
{
    public UserSessionByRefreshTokenHashSpecification(string refreshTokenHash)
        : base(session => session.RefreshTokenHash == refreshTokenHash)
    {
    }
}

