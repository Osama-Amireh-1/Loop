using Loop.SharedKernel;

namespace Loop.Domain.Common;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public const string DefaultCurrency = "JOD";

    private static readonly Error NegativeAmountError = new(
        "Common.Money.NegativeAmount",
        "Money amount cannot be negative.",
        ErrorType.Validation);

    private static readonly Error CurrencyMismatchError = new(
        "Common.Money.CurrencyMismatch",
        "Cannot perform the operation on different currencies.",
        ErrorType.Validation);

    private static readonly Error NegativeResultError = new(
        "Common.Money.NegativeResult",
        "Resulting amount cannot be negative.",
        ErrorType.Validation);

    private Money(decimal amount, string currency = DefaultCurrency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Result<Money> Create(decimal amount)
    {
        if (amount < 0)
        {
            return Result.Failure<Money>(NegativeAmountError);
        }

        return Result.Success(new Money(Math.Round(amount, 2)));
    }

    public static Money Zero() => new(0m);

    public Result<Money> Add(Money other)
    {
        if (Currency != other.Currency)
        {
            return Result.Failure<Money>(CurrencyMismatchError);
        }

        return Result.Success(new Money(Amount + other.Amount, Currency));
    }

    public Result<Money> Subtract(Money other)
    {
        if (Currency != other.Currency)
        {
            return Result.Failure<Money>(CurrencyMismatchError);
        }

        if (Amount - other.Amount < 0)
        {
            return Result.Failure<Money>(NegativeResultError);
        }

        return Result.Success(new Money(Amount - other.Amount, Currency));
    }

    public override string ToString() => $"{Amount:F2} {Currency}";

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }
}


