using Loop.Domain.Common;
using Loop.Domain.Malls;
using Loop.SharedKernel;

namespace Loop.Domain.Configuration;

public class SystemConfig : AggregateRoot
{
    public Guid ConfigId { get; private set; }
    public Guid MallId { get; private set; }
    public decimal PointsToCurrencyRatio { get; private set; }
    public decimal EarnPointsPerCurrency { get; private set; }
    public int MinRedemptionThreshold { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedByAdminId { get; private set; }

    private static readonly Error InvalidRatioError = new(
        "Configuration.InvalidRatio",
        "Ratio must be positive.",
        ErrorType.Validation);

    private static readonly Error InvalidEarnRateError = new(
        "Configuration.InvalidEarnRate",
        "Earn rate must be positive.",
        ErrorType.Validation);

    private static readonly Error InvalidThresholdError = new(
        "Configuration.InvalidThreshold",
        "Threshold cannot be negative.",
        ErrorType.Validation);

    private SystemConfig() { }

    public static SystemConfig Create(
        Guid mallId,
        Guid adminId,
        decimal pointsToCurrencyRatio,
        decimal earnPointsPerCurrency)
        => new()
        {
            ConfigId = Guid.NewGuid(),
            MallId = mallId,
            PointsToCurrencyRatio = pointsToCurrencyRatio,
            EarnPointsPerCurrency = earnPointsPerCurrency,
            MinRedemptionThreshold = 0,
            UpdatedAt = DateTime.UtcNow,
            UpdatedByAdminId = adminId
        };

    public Result Update(
        decimal ratio,
        decimal earnRate,
        int minThreshold,
        Guid adminId)
    {
        if (ratio <= 0)
        {
            return Result.Failure(InvalidRatioError);
        }

        if (earnRate <= 0)
        {
            return Result.Failure(InvalidEarnRateError);
        }

        if (minThreshold < 0)
        {
            return Result.Failure(InvalidThresholdError);
        }

        PointsToCurrencyRatio = ratio;
        EarnPointsPerCurrency = earnRate;
        MinRedemptionThreshold = minThreshold;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByAdminId = adminId;
        return Result.Success();
    }

    public int CalculateEarnedPoints(decimal purchaseAmount)
        => (int)(purchaseAmount * EarnPointsPerCurrency);

    public Result<Money> CalculateDiscountValue(int points)
        => Money.Create(points * PointsToCurrencyRatio);
}



