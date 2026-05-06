using System.Text.RegularExpressions;
using Loop.SharedKernel;

namespace Loop.Domain.Common;

public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Error EmptyEmailError = new(
        "Common.Email.Empty",
        "Email cannot be empty.",
        ErrorType.Validation);

    private static readonly Error InvalidEmailError = new(
        "Common.Email.Invalid",
        "The provided email address is invalid.",
        ErrorType.Validation);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Email>(EmptyEmailError);
        }

        email = email.Trim().ToUpperInvariant();

        if (!EmailRegex.IsMatch(email))
        {
            return Result.Failure<Email>(InvalidEmailError);
        }

        return Result.Success(new Email(email));
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}



