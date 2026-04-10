using Loop.SharedKernel;

namespace Loop.Domain.Users;

public class UserSession : AggregateRoot
{
    public Guid SessionId { get; private set; }
    public Guid UserId { get; private set; }
    public string RefreshTokenHash { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private UserSession() { }

    public static UserSession Create(Guid userId, string refreshTokenHash, DateTime expiresAtUtc)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId is required.");

        if (string.IsNullOrWhiteSpace(refreshTokenHash))
            throw new DomainException("Refresh token hash is required.");

        if (expiresAtUtc <= DateTime.UtcNow)
            throw new DomainException("Session expiry must be in the future.");

        return new UserSession
        {
            SessionId = Guid.NewGuid(),
            UserId = userId,
            RefreshTokenHash = refreshTokenHash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsExpired() => ExpiresAtUtc <= DateTime.UtcNow;
}


