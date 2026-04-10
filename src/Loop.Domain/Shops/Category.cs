using Loop.SharedKernel;

namespace Loop.Domain.Shops;

public class Category : AggregateRoot
{
    private readonly List<Shop> _shops = new();
    public Guid MallId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public string IconUrl { get; private set; }
    public int DisplayOrder { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<Shop> Shops => _shops.AsReadOnly();

    private Category() { }

    public static Category Create(
        Guid mallId,
        string name,
        string iconUrl,
        string description,
        int displayOrder)
        => new()
        {
            CategoryId = Guid.NewGuid(),
            MallId= mallId,
            Name = name,
            IconUrl = iconUrl,
            Description = description,
            DisplayOrder = displayOrder,
            CreatedAt = DateTime.UtcNow
        };

    public void UpdateDisplayOrder(int order) => DisplayOrder = order;
}


