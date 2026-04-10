using Loop.SharedKernel;

namespace Loop.Domain.Stamps;

public static class StampErrors
{
    public static Error NotFound(Guid stampId) => Error.NotFound(
        "Stamps.NotFound",
        $"The stamp with the Id = '{stampId}' was not found");

    public static readonly Error StampInactive = Error.Failure(
        "Stamps.StampInactive",
        "The stamp program is currently inactive");

    public static readonly Error StampExpired = Error.Failure(
        "Stamps.StampExpired",
        "The stamp program has expired or is not yet active");

    public static readonly Error CardAlreadyCompleted = Error.Failure(
        "Stamps.CardAlreadyCompleted",
        "The stamp card is already completed");

    public static Error CardNotFound(Guid userId, Guid stampId) => Error.NotFound(
        "Stamps.CardNotFound",
        $"No stamp card found for user '{userId}' and stamp '{stampId}'");

    public static readonly Error CardNotCompleted = Error.Failure(
        "Stamps.CardNotCompleted",
        "The stamp card is not yet completed for reward redemption");
}


