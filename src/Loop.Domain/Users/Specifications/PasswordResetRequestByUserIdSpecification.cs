using Loop.Domain.Specifications;

namespace Loop.Domain.Users.Specifications;

public sealed class PasswordResetRequestByUserIdSpecification : Specification<PasswordResetRequest>
{
    public PasswordResetRequestByUserIdSpecification(Guid userId)
        : base(request => request.UserId == userId)
    {
    }
}

