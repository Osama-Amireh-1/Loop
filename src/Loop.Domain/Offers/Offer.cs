using Loop.Domain.Shops;
using Loop.SharedKernel;

namespace Loop.Domain.Offers;

public class Offer : AggregateRoot
{
    public Guid OfferId { get; private set; }
    public Guid ShopId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string ImageUrl { get; private set; }
    public RewardType RewardType { get; private set; }
    public string RewardValue { get; private set; }  
    public bool IsActive { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Shop Shop { get; private set; }

    private readonly List<OfferRedemption> _redemptions = [];
    public IReadOnlyList<OfferRedemption> Redemptions => _redemptions.AsReadOnly();

    private Offer() { }

    public static Offer Create(
        Guid shopId,
        string name,
        string description,
        RewardType rewardType,
        string rewardValue,
        DateTime startDate,
        DateTime endDate)
        => new()
        {
            OfferId = Guid.NewGuid(),
            ShopId = shopId,
            Name = name,
            Description = description,
            RewardType = rewardType,
            RewardValue = rewardValue,
            IsActive = true,
            StartDate = startDate,
            EndDate = endDate,
            CreatedAt = DateTime.UtcNow
        };

    public OfferRedemption Redeem(Guid userId, Guid shopId, Guid? receiptId)
    {
        if (!IsActive)
            throw new DomainException("Offer is not active.");
        if (DateTime.UtcNow < StartDate ||  DateTime.UtcNow > EndDate)
            throw new DomainException("Offer is outside its active period.");

        var redemption = OfferRedemption.Create(OfferId, userId, shopId, receiptId);
        _redemptions.Add(redemption);
        return redemption;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}



