using System.Text.RegularExpressions;
using Loop.SharedKernel;

namespace Loop.Domain.Common;

public sealed class Email:ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        email = email.Trim().ToUpperInvariant();

        if (!EmailRegex.IsMatch(email))
        {
            throw new ArgumentException($"'{email}' is not a valid email address.", nameof(email));
        }

        return new Email(email);
    }

    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}


