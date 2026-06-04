using Loop.Domain.Stamps;
using Loop.SharedKernel;
using Shouldly;

namespace Loop.ArchitectureTests.Domain.Stamps;

public class StampTests
{
    [Fact]
    public void Create_ShouldThrow_WhenStampsRequiredIsNotPositive()
    {
        var exception = Should.Throw<DomainException>(() =>
            Stamp.Create(
                Guid.NewGuid(),
                "Coffee Card",
                0,
                StampType.Collect,
                DateTime.UtcNow,
                DateTime.UtcNow.AddDays(7)));

        exception.Message.ShouldBe("Stamps required must be greater than zero.");
    }

    [Fact]
    public void Create_ShouldInitializeActiveStamp_WhenInputsAreValid()
    {
        var startDate = DateTime.UtcNow.AddHours(-1);
        var endDate = DateTime.UtcNow.AddDays(7);

        var stamp = Stamp.Create(
            Guid.NewGuid(),
            "Coffee Card",
            5,
            StampType.Reward,
            startDate,
            endDate);

        stamp.StampId.ShouldNotBe(Guid.Empty);
        stamp.IsActive.ShouldBeTrue();
        stamp.StampsRequired.ShouldBe(5);
        stamp.RewardType.ShouldBe(StampType.Reward);
        stamp.StartDate.ShouldBe(startDate);
        stamp.EndDate.ShouldBe(endDate);
        stamp.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void DeactivateAndActivate_ShouldToggleStampState()
    {
        var stamp = Stamp.Create(
            Guid.NewGuid(),
            "Coffee Card",
            3,
            StampType.Collect,
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddDays(1));

        stamp.Deactivate();
        stamp.IsActive.ShouldBeFalse();

        stamp.Activate();
        stamp.IsActive.ShouldBeTrue();
    }
}