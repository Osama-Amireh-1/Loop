using Loop.Domain.Specifications;

namespace Loop.Domain.Users.Specifications;

public sealed class UserSessionsByUserIdSpecification : Specification<UserSession>
{
    public UserSessionsByUserIdSpecification(Guid userId)
        : base(session => session.UserId == userId)
    {
    }
}

