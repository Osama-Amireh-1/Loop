using Loop.Domain.Common;
using Loop.Domain.Transactions;
using Shouldly;

namespace Loop.ArchitectureTests.Domain.Transactions;

public class RedeemTransactionTests
{
    [Fact]
    public void Initiate_ShouldCreatePendingTransaction_WithVerificationCodeAndTimestamp()
    {
        var discount = Money.Create(2.5m).Value;

        var transaction = RedeemTransaction.Initiate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            discount,
            40m);

        transaction.Status.ShouldBe(RedemptionStatus.Pending);
        transaction.CompletedAt.ShouldBeNull();
        transaction.VerificationCode.Length.ShouldBe(6);
        int.TryParse(transaction.VerificationCode, out _).ShouldBeTrue();
        transaction.PointsUsed.ShouldBe(100);
        transaction.DiscountValue.ShouldBe(discount);
        transaction.AppliedPointsToCurrencyRatio.ShouldBe(40m);
        transaction.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void Verify_ShouldSucceed_WhenStatusIsPending()
    {
        var transaction = CreatePending();
        var before = transaction.CompletedAt;

        var result = transaction.Verify();

        result.IsSuccess.ShouldBeTrue();
        transaction.Status.ShouldBe(RedemptionStatus.Verified);
        transaction.CompletedAt.ShouldNotBeNull();
        transaction.CompletedAt.ShouldNotBe(before);
    }

    [Fact]
    public void Verify_ShouldFail_WhenTransactionAlreadyProcessed()
    {
        var transaction = CreatePending();
        _ = transaction.Verify();
        var completedAt = transaction.CompletedAt;

        var result = transaction.Verify();

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Transactions.RedemptionAlreadyProcessed");
        transaction.Status.ShouldBe(RedemptionStatus.Verified);
        transaction.CompletedAt.ShouldBe(completedAt);
    }

    [Fact]
    public void Cancel_ShouldSucceed_WhenStatusIsPending()
    {
        var transaction = CreatePending();
        var before = transaction.CompletedAt;

        var result = transaction.Cancel();

        result.IsSuccess.ShouldBeTrue();
        transaction.Status.ShouldBe(RedemptionStatus.Cancelled);
        transaction.CompletedAt.ShouldNotBeNull();
        transaction.CompletedAt.ShouldNotBe(before);
    }

    [Fact]
    public void Cancel_ShouldFail_WhenTransactionAlreadyProcessed()
    {
        var transaction = CreatePending();
        _ = transaction.Cancel();
        var completedAt = transaction.CompletedAt;

        var result = transaction.Cancel();

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Transactions.RedemptionAlreadyProcessed");
        transaction.Status.ShouldBe(RedemptionStatus.Cancelled);
        transaction.CompletedAt.ShouldBe(completedAt);
    }

    private static RedeemTransaction CreatePending() =>
        RedeemTransaction.Initiate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            50,
            Money.Create(1.25m).Value,
            40m);
}
