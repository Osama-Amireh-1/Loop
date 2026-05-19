namespace Loop.SharedKernel;

public record Error
{
    public static readonly Error None = new(string.Empty, "No error occurred", ErrorType.Failure);
    public static readonly Error NullValue = new(
        "General.Null",
        "A null value was provided.",
        ErrorType.Failure);

    public Error(string code, string description, ErrorType type)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Error code cannot be null or whitespace.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Error description cannot be null or whitespace.", nameof(description));
        }

        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }

    public string Description { get; }

    public ErrorType Type { get; }

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
}

