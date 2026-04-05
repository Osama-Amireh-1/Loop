using SharedKernel;

namespace Domain.Users;

public static class UserPointsBalanceErrors
{
    public static readonly Error InsufficientPoints = Error.Failure(
        "Users.InsufficientPoints",
        "The user does not have enough points for this operation");
}
