using Loop.SharedKernel;
using SharedKernel;

namespace Domain.Users;

public class UserPointsBalance : Entity
{
    public Guid UserPointsBalanceId { get; private set; }
    public Guid UserId { get; private set; }
    public int TotalPoints { get; private set; }
    public int LifetimePoints { get; private set; }
    public DateTime LastUpdated { get; private set; }

    private UserPointsBalance() { }

    internal static UserPointsBalance Create(Guid userId) => new()
    {
        UserPointsBalanceId = Guid.NewGuid(),
        UserId = userId,
        TotalPoints = 0,
        LifetimePoints = 0,
        LastUpdated = DateTime.UtcNow
    };

    internal void Credit(int amount)
    {
        TotalPoints += amount;
        LifetimePoints += amount;
        LastUpdated = DateTime.UtcNow;
    }

    internal void Debit(int amount)
    {
        if (TotalPoints < amount)
            throw new DomainException("Insufficient points.");
        TotalPoints -= amount;
        LastUpdated = DateTime.UtcNow;
    }
}
