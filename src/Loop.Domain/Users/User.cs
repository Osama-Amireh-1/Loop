using Loop.Domain.Common;
using Loop.SharedKernel;

namespace Loop.Domain.Users;

public class User : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Phone Phone { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Gender Gender { get; private set; }
    public string? ProfileImageUrl { get; private set; }
    public Guid TierId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public UserPointsBalance PointsBalance { get; private set; }

    private User() { }

    public static User Create(
        string firstName,
        string lastName,
        Phone phone,
        Email email,
        string passwordHash,
        Gender gender,
        Guid defaultTierId)
    {
        var userId = Guid.NewGuid();
        return new User
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            Email = email,
            PasswordHash = passwordHash,
            Gender = gender,
            TierId = defaultTierId,
            CreatedAt = DateTime.UtcNow,
            PointsBalance = UserPointsBalance.Create(userId)
        };
    }

    public void UpdateProfile(string firstName, string lastName, string? profileImageUrl)
    {
        FirstName = firstName;
        LastName = lastName;
        ProfileImageUrl = profileImageUrl;
    }

    public void ChangePasswordHash(string newHash) => PasswordHash = newHash;
    public void UpgradeTier(Guid newTierId) => TierId = newTierId;

    public void CreditPoints(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Credit amount must be positive.");
        PointsBalance.Credit(amount);
    }

    public void DebitPoints(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Debit amount must be positive.");
        PointsBalance.Debit(amount);
    }

    public bool HasEnoughPoints(int required)
        => PointsBalance.TotalPoints >= required;
}


