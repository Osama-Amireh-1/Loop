using Loop.Domain.Configuration;
using Loop.SharedKernel;

namespace Loop.Domain.Malls;

public class Mall : AggregateRoot
{
    public Guid MallId { get; private set; }
    public string Name { get; private set; }
    public string? Location { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public SystemConfig SystemConfig { get; private set; }

    private Mall() { }

    public static Mall Create(string name, string? location, string? logoUrl)
        => new()
        {
            MallId = Guid.NewGuid(),
            Name = name,
            Location = location,
            LogoUrl = logoUrl,
            CreatedAt = DateTime.UtcNow
        };

    public void UpdateDetails(
        string name,
        string? location,
        string? logoUrl,
        string? coverImageUrl)
    {
        Name = name;
        Location = location;
        LogoUrl = logoUrl;
        CoverImageUrl = coverImageUrl;
    }
}


