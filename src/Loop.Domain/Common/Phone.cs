using Loop.SharedKernel;

namespace Loop.Domain.Common;

public sealed class Phone : ValueObject
{
    private static readonly Error EmptyPhoneError = new(
        "Common.Phone.Empty",
        "Phone number cannot be empty.",
        ErrorType.Validation);

    private static readonly Error InvalidPhoneLengthError = new(
        "Common.Phone.InvalidLength",
        "Phone number must be between 7 and 20 characters.",
        ErrorType.Validation);

    public string Value { get; }

    private Phone(string value) => Value = value;

    public static Result<Phone> Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return Result.Failure<Phone>(EmptyPhoneError);
        }

        phone = phone.Trim();

        if (phone.Length < 7 || phone.Length > 20)
        {
            return Result.Failure<Phone>(InvalidPhoneLengthError);
        }

        return Result.Success(new Phone(phone));
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}



