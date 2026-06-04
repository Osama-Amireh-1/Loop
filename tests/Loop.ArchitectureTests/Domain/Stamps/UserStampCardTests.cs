using Loop.Domain.Stamps;
using Loop.SharedKernel;
using Shouldly;

namespace Loop.ArchitectureTests.Domain.Stamps;

public class UserStampCardTests
{
    [Fact]
    public void Open_ShouldInitializeAnUncompletedCard()
    {
        var userId = Guid.NewGuid();
        var stampId = Guid.NewGuid();

        var card = UserStampCard.Open(userId, stampId);

        card.UserId.ShouldBe(userId);
        card.StampId.ShouldBe(stampId);
        card.StampsCounter.ShouldBe(0);
        card.IsCompleted.ShouldBeFalse();
        card.LastTransaction.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        card.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void CollectStamp_ShouldIncrementCounterAndMarkCompleted_WhenThresholdIsReached()
    {
        var card = UserStampCard.Open(Guid.NewGuid(), Guid.NewGuid());

        card.CollectStamp(stampsRequired: 3, count: 2);

        card.StampsCounter.ShouldBe(2);
        card.IsCompleted.ShouldBeFalse();

        card.CollectStamp(stampsRequired: 3, count: 1);

        card.StampsCounter.ShouldBe(3);
        card.IsCompleted.ShouldBeTrue();
        card.LastTransaction.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CollectStamp_ShouldThrow_WhenCountIsNotPositive(int count)
    {
        var card = UserStampCard.Open(Guid.NewGuid(), Guid.NewGuid());

        var exception = Should.Throw<DomainException>(() => card.CollectStamp(stampsRequired: 5, count: count));

        exception.Message.ShouldBe("Stamp count must be positive.");
        card.StampsCounter.ShouldBe(0);
        card.IsCompleted.ShouldBeFalse();
    }

    [Fact]
    public void CollectStamp_ShouldThrow_WhenCardIsAlreadyCompleted()
    {
        var card = UserStampCard.Open(Guid.NewGuid(), Guid.NewGuid());
        card.CollectStamp(stampsRequired: 1, count: 1);

        var exception = Should.Throw<DomainException>(() => card.CollectStamp(stampsRequired: 1, count: 1));

        exception.Message.ShouldBe("Stamp card is already completed.");
    }
}