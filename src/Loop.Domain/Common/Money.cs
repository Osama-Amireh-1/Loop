using Loop.SharedKernel;

namespace Loop.Domain.Common;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public const string DefaultCurrency = "JOD";

    private Money(decimal amount, string currency = DefaultCurrency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentException("Money amount cannot be negative.", nameof(amount));

        return new Money(Math.Round(amount, 2));
    }

    public static Money Zero() => new(0m);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money of different currencies.");

        return new(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract money of different currencies.");

        if (Amount - other.Amount < 0)
            throw new InvalidOperationException("Resulting amount cannot be negative.");

        return new(Amount - other.Amount, Currency);
    }

    public override string ToString() => $"{Amount:F2} {Currency}";

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }
}


