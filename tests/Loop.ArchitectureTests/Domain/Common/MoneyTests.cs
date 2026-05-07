using Loop.Domain.Common;
using Shouldly;

namespace Loop.ArchitectureTests.Domain.Common;

public class MoneyTests
{
    [Fact]
    public void Create_ShouldFail_WhenAmountIsNegative()
    {
        var result = Money.Create(-0.01m);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.Money.NegativeAmount");
    }

    [Fact]
    public void Create_ShouldRoundAmount_ToTwoDecimalPlaces()
    {
        var result = Money.Create(1.235m);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Amount.ShouldBe(1.24m);
        result.Value.Currency.ShouldBe(Money.DefaultCurrency);
    }

    [Fact]
    public void Add_ShouldReturnCorrectSum()
    {
        var left = Money.Create(10m).Value;
        var right = Money.Create(2.5m).Value;

        var result = left.Add(right);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Amount.ShouldBe(12.5m);
        result.Value.Currency.ShouldBe(Money.DefaultCurrency);
    }

    [Fact]
    public void Subtract_ShouldFail_WhenResultWouldBeNegative()
    {
        var left = Money.Create(3m).Value;
        var right = Money.Create(5m).Value;

        var result = left.Subtract(right);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.Money.NegativeResult");
    }

    [Fact]
    public void Zero_ShouldReturnMoneyWithDefaultCurrency()
    {
        var result = Money.Zero();

        result.Amount.ShouldBe(0m);
        result.Currency.ShouldBe(Money.DefaultCurrency);
    }
}
