using Loop.SharedKernel;

namespace Loop.Domain.Configuration;

public static class SystemConfigErrors
{
    public static Error NotFound(Guid configId) => Error.NotFound(
        "Configuration.NotFound",
        $"The system config with the Id = '{configId}' was not found");

    public static readonly Error InvalidRatio = Error.Failure(
        "Configuration.InvalidRatio",
        "Ratio must be positive");

    public static readonly Error InvalidEarnRate = Error.Failure(
        "Configuration.InvalidEarnRate",
        "Earn rate must be positive");

    public static readonly Error InvalidMinThreshold = Error.Failure(
        "Configuration.InvalidMinThreshold",
        "Threshold cannot be negative");
}


