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

    private static readonly Error InvalidCreditAmountError = new(
        "Users.InvalidCreditAmount",
        "Credit amount must be positive.",
        ErrorType.Validation);

    private static readonly Error InvalidDebitAmountError = new(
        "Users.InvalidDebitAmount",
        "Debit amount must be positive.",
        ErrorType.Validation);

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

    public void UpdateProfile(string firstName, string lastName, string? profileImageUrl, Phone? phone = null)
        => UpdateProfile(firstName, lastName, Gender, profileImageUrl, phone);

    public void UpdateProfile(string firstName, string lastName, Gender gender, string? profileImageUrl, Phone? phone = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Gender = gender;
        ProfileImageUrl = profileImageUrl;

        if (phone is not null)
        {
            Phone = phone;
        }
    }

    public Result CreditPoints(int amount)
    {
        if (amount <= 0)
        {
            return Result.Failure(InvalidCreditAmountError);
        }

        PointsBalance.Credit(amount);
        return Result.Success();
    }

    public Result DebitPoints(int amount)
    {
        if (amount <= 0)
        {
            return Result.Failure(InvalidDebitAmountError);
        }

        PointsBalance.Debit(amount);
        return Result.Success();
    }

    public void ChangePasswordHash(string newHash) => PasswordHash = newHash;
    public void UpgradeTier(Guid newTierId) => TierId = newTierId;

    public bool HasEnoughPoints(int required)
        => PointsBalance.TotalPoints >= required;
}


