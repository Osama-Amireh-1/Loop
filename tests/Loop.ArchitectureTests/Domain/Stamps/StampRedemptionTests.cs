using Loop.Domain.Stamps;
using Shouldly;

namespace Loop.ArchitectureTests.Domain.Stamps;

public class StampRedemptionTests
{
    [Fact]
    public void Create_ShouldSetAllFields()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var stampId = Guid.NewGuid();
        var qrId = Guid.NewGuid();

        var redemption = StampRedemption.Create(userId, shopId, stampId, qrId);

        redemption.RedemptionId.ShouldNotBe(Guid.Empty);
        redemption.UserId.ShouldBe(userId);
        redemption.ShopId.ShouldBe(shopId);
        redemption.StampId.ShouldBe(stampId);
        redemption.QrId.ShouldBe(qrId);
    }
}
