using Loop.Domain.Offers;
using Loop.Domain.Shops;
using Loop.SharedKernel;

namespace Loop.Domain.Stamps;

public class Stamp : AggregateRoot
{
    public Guid StampId { get; private set; }
    public Guid ShopId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string ImageUrl { get; private set; }
    public string StampIconUrl { get; private set; }
    public int StampsRequired { get; private set; }
    public StampType RewardType { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Shop Shop { get; private set; }

    private readonly List<StampRedemption> _redemptions = [];
    public IReadOnlyList<StampRedemption> Redemptions => _redemptions.AsReadOnly();

    private Stamp() { }

    public static Stamp Create(
        Guid shopId,
        string name,
        int stampsRequired,
        StampType rewardType,
        DateTime startDate,
        DateTime endDate)
    {
        if (stampsRequired <= 0)
            throw new DomainException("Stamps required must be greater than zero.");
        return new()
        {
            StampId = Guid.NewGuid(),
            ShopId = shopId,
            Name = name,
            StampsRequired = stampsRequired,
            RewardType = rewardType,
            IsActive = true,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}


