using Domain.Common;
using Loop.SharedKernel;

namespace Domain.Shops;

public class ShopAdmin : AggregateRoot
{
    public Guid ShopAdminId { get; private set; }
    public Guid ShopId { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Phone Phone { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }


    private ShopAdmin() { }

    public static ShopAdmin Create(
        Guid shopId,
        string name,
        Email email,
        Phone phone,
        string passwordHash)
        => new()
        {
            ShopAdminId = Guid.NewGuid(),
            ShopId = shopId,
            Name = name,
            Email = email,
            Phone = phone,
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void ChangePasswordHash(string hash) => PasswordHash = hash;
}
