using Domain.Users;
using Loop.SharedKernel;
using SharedKernel;

namespace Domain.Stamps;

public class UserStampCard : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid StampId { get; private set; }
    public int StampsCounter { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime LastTransaction { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public Stamp Stamp { get; private set; }

    private UserStampCard() { }

    public static UserStampCard Open(Guid userId, Guid stampProgramId)
        => new()
        {
            UserId = userId,
            StampId = stampProgramId,
            StampsCounter = 0,
            IsCompleted = false,
            LastTransaction = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

    public void CollectStamp(int stampsRequired, int count = 1)
    {
        if (IsCompleted)
            throw new DomainException("Stamp card is already completed.");
        if (count <= 0)
            throw new DomainException("Stamp count must be positive.");

        StampsCounter += count;
        LastTransaction = DateTime.UtcNow;

        if (StampsCounter >= stampsRequired)
        {
            IsCompleted = true;
        }
    }
}
