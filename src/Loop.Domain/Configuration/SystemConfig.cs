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

    public void Update(
        decimal ratio,
        decimal earnRate,
        int minThreshold,
        Guid adminId)
    {
        if (ratio <= 0)
            throw new DomainException("Ratio must be positive.");
        if (earnRate <= 0)
            throw new DomainException("Earn rate must be positive.");
        if (minThreshold < 0)
            throw new DomainException("Threshold cannot be negative.");

        PointsToCurrencyRatio = ratio;
        EarnPointsPerCurrency = earnRate;
        MinRedemptionThreshold = minThreshold;
        UpdatedAt = DateTime.UtcNow;
        UpdatedByAdminId = adminId;
    }

    public int CalculateEarnedPoints(decimal purchaseAmount)
        => (int)(purchaseAmount * EarnPointsPerCurrency);

    public Money CalculateDiscountValue(int points)
        => Money.Create(points * PointsToCurrencyRatio);
}



