using Loop.SharedKernel;

namespace Loop.Domain.Malls;

public class MallAdminSession : AggregateRoot
{
    public Guid SessionId { get; private set; }
    public Guid MallAdminId { get; private set; }
    public string RefreshTokenHash { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private MallAdminSession() { }

    public static MallAdminSession Create(Guid mallAdminId, string refreshTokenHash, DateTime expiresAtUtc)
    {
        if (mallAdminId == Guid.Empty)
            throw new DomainException("MallAdminId is required.");

        if (string.IsNullOrWhiteSpace(refreshTokenHash))
            throw new DomainException("Refresh token hash is required.");

        if (expiresAtUtc <= DateTime.UtcNow)
            throw new DomainException("Session expiry must be in the future.");

        return new MallAdminSession
        {
            SessionId = Guid.NewGuid(),
            MallAdminId = mallAdminId,
            RefreshTokenHash = refreshTokenHash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsExpired() => ExpiresAtUtc <= DateTime.UtcNow;
}
