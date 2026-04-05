using SharedKernel;

namespace Domain.Tiers;

public static class TierErrors
{
    public static Error NotFound(Guid tierId) => Error.NotFound(
        "Tiers.NotFound",
        $"The tier with the Id = '{tierId}' was not found");

    public static Error NotFound(int tierOrder) => Error.NotFound(
      "Tiers.NotFound",
      $"The tier with the Order = '{tierOrder}' was not found");

    public static readonly Error NameNotUnique = Error.Conflict(
        "Tiers.NameNotUnique",
        "A tier with the specified name already exists");

    public static readonly Error InvalidPointsRequired = Error.Failure(
        "Tiers.InvalidPointsRequired",
        "Points required must be a non-negative value");
}
