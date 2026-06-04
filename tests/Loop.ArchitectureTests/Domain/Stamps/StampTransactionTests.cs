using Loop.Domain.Stamps;
using Shouldly;

namespace Loop.ArchitectureTests.Domain.Stamps;

public class StampTransactionTests
{
    [Fact]
    public void RecordCollect_ShouldSetCollectTransactionFields()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var stampProgramId = Guid.NewGuid();
        var qrId = Guid.NewGuid();

        var transaction = StampTransaction.RecordCollect(userId, shopId, stampProgramId, 3, qrId);

        transaction.StampTxId.ShouldNotBe(Guid.Empty);
        transaction.UserId.ShouldBe(userId);
        transaction.ShopId.ShouldBe(shopId);
        transaction.StampProgramId.ShouldBe(stampProgramId);
        transaction.Type.ShouldBe(StampType.Collect);
        transaction.StampsCount.ShouldBe(3);
        transaction.RedemptionRef.ShouldBe(qrId);
        transaction.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void RecordReward_ShouldSetRewardTransactionFields()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var stampProgramId = Guid.NewGuid();

        var transaction = StampTransaction.RecordReward(userId, shopId, stampProgramId);

        transaction.StampTxId.ShouldNotBe(Guid.Empty);
        transaction.UserId.ShouldBe(userId);
        transaction.ShopId.ShouldBe(shopId);
        transaction.StampProgramId.ShouldBe(stampProgramId);
        transaction.Type.ShouldBe(StampType.Reward);
        transaction.StampsCount.ShouldBe(0);
        transaction.RedemptionRef.ShouldBeNull();
        transaction.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }
}