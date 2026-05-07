using Loop.Domain.Offers;
using Loop.SharedKernel;
using Shouldly;

namespace Loop.ArchitectureTests.Domain.Offers;

public class OfferTests
{
    [Fact]
    public void Redeem_ShouldCreateRedemption_WhenOfferIsActiveAndInDateRange()
    {
        var now = DateTime.UtcNow;
        var offer = Offer.Create(
            Guid.NewGuid(),
            "Discount",
            "Description",
            RewardType.Discount,
            "10%",
            now.AddHours(-1),
            now.AddHours(1));

        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var receiptId = Guid.NewGuid();

        var redemption = offer.Redeem(userId, shopId, receiptId);

        redemption.RedemptionId.ShouldNotBe(Guid.Empty);
        redemption.OfferId.ShouldBe(offer.OfferId);
        redemption.UserId.ShouldBe(userId);
        redemption.ShopId.ShouldBe(shopId);
        redemption.RedemptionRef.ShouldBe(receiptId);
        offer.Redemptions.Count.ShouldBe(1);
        offer.Redemptions[0].RedemptionId.ShouldBe(redemption.RedemptionId);
    }

    [Fact]
    public void Redeem_ShouldThrow_WhenOfferIsInactive()
    {
        var now = DateTime.UtcNow;
        var offer = Offer.Create(
            Guid.NewGuid(),
            "Discount",
            "Description",
            RewardType.Discount,
            "10%",
            now.AddHours(-1),
            now.AddHours(1));
        offer.Deactivate();

        var exception = Should.Throw<DomainException>(() =>
            offer.Redeem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        exception.Message.ShouldBe("Offer is not active.");
    }

    [Fact]
    public void Redeem_ShouldThrow_WhenOfferIsOutsideActivePeriod()
    {
        var now = DateTime.UtcNow;
        var offer = Offer.Create(
            Guid.NewGuid(),
            "Discount",
            "Description",
            RewardType.Discount,
            "10%",
            now.AddHours(1),
            now.AddHours(2));

        var exception = Should.Throw<DomainException>(() =>
            offer.Redeem(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));

        exception.Message.ShouldBe("Offer is outside its active period.");
    }
}
