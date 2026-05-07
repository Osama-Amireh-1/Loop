using Loop.Domain.Common;
using Loop.Domain.Transactions;
using Shouldly;

namespace Loop.ArchitectureTests.Domain.Transactions;

public class EarnTransactionTests
{
    [Fact]
    public void Record_ShouldSetAllFields()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var amount = Money.Create(15.75m).Value;

        var transaction = EarnTransaction.Record(
            userId,
            shopId,
            amount,
            30,
            "tx-ref-1");

        transaction.EarnId.ShouldNotBe(Guid.Empty);
        transaction.UserId.ShouldBe(userId);
        transaction.ShopId.ShouldBe(shopId);
        transaction.PurchaseAmount.ShouldBe(amount);
        transaction.PointsEarned.ShouldBe(30);
        transaction.TransactionRef.ShouldBe("tx-ref-1");
    }
}
