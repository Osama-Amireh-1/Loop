using Loop.SharedKernel;

namespace Loop.Domain.Stamps;

public static class UserStampCardErrors
{
    public static Error NotFound(Guid userId, Guid stampId) => Error.NotFound(
        "Stamps.CardNotFound",
        $"No stamp card found for user '{userId}' and stamp '{stampId}'");

    public static readonly Error AlreadyCompleted = Error.Failure(
        "Stamps.CardAlreadyCompleted",
        "The stamp card is already completed");
}


