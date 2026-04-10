namespace Loop.SharedKernel;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

