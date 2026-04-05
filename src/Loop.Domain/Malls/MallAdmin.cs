using Domain.Common;
using Loop.SharedKernel;

namespace Domain.Malls;

public class MallAdmin : AggregateRoot
{
    public Guid MallAdminId { get; private set; }
    public Guid MallId { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Phone Phone { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private MallAdmin() { }

    public static MallAdmin Create(
        Guid mallId,
        string name,
        Email email,
        Phone phone,
        string passwordHash)
        => new()
        {
            MallAdminId = Guid.NewGuid(),
            MallId = mallId,
            Name = name,
            Email = email,
            Phone = phone,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };

    public void ChangePasswordHash(string hash) => PasswordHash = hash;
}
