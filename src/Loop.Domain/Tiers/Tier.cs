using Loop.SharedKernel;

namespace Domain.Tiers;

public class Tier : AggregateRoot
{
    public Guid TierId { get; private set; }
    public int TierOrder { get; private set; }
    public string Name { get; private set; }
    public int PointsRequired { get; private set; }
    public string? Benefits { get; private set; }   
    public string? IconUrl { get; private set; }
    public string? ColorHex { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Tier() { }

    public static Tier Create(
        string name,
        int pointsRequired,
        string? benefits,
        string? iconUrl,
        string? colorHex)
        => new()
        {
            TierId = Guid.NewGuid(),
            Name = name,
            PointsRequired = pointsRequired,
            Benefits = benefits,
            IconUrl = iconUrl,
            ColorHex = colorHex,
            CreatedAt = DateTime.UtcNow
        };

    public void Update(string name, int pointsRequired, string? benefits)
    {
        Name = name;
        PointsRequired = pointsRequired;
        Benefits = benefits;
    }
}
