using Loop.SharedKernel;

namespace Loop.Domain.Common;

public sealed class Phone : ValueObject
{
    public string Value { get; }

    private Phone(string value) => Value = value;

    public static Phone Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("Phone number cannot be empty.", nameof(phone));
        }

        phone = phone.Trim();

        if (phone.Length < 7 || phone.Length > 20)
        {
            throw new ArgumentException("Phone number must be between 7 and 20 characters.", nameof(phone));
        }

        return new Phone(phone);
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}


