using Loop.SharedKernel;

namespace Loop.Domain.Users;

public class PasswordResetRequest : AggregateRoot
{
    public Guid RequestId { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }

    private PasswordResetRequest() { }

    public static PasswordResetRequest Create(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("Token hash is required.");

        if (expiresAtUtc <= DateTime.UtcNow)
            throw new DomainException("Reset token expiry must be in the future.");

        return new PasswordResetRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public bool IsExpired() => ExpiresAtUtc <= DateTime.UtcNow;
}


