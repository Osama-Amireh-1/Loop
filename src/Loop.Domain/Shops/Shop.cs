using Loop.Domain.Malls;
using Loop.SharedKernel;

namespace Loop.Domain.Shops;

public class Shop : AggregateRoot
{
    public Guid ShopId { get; private set; }
    public Guid MallId { get; private set; }
    public string Name { get; private set; }
    public Guid CategoryId { get; private set; }
    public string LogoUrl { get; private set; }
    public string CoverImageUrl { get; private set; }
    public string Bio { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public string? SocialLinks { get; private set; }   
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Category Category { get; private set; }

    private Shop() { }

    public static Shop Create(
        Guid mallId,
        string name,
        Guid categoryId)
        => new()
        {
            ShopId = Guid.NewGuid(),
            MallId = mallId,
            Name = name,
            CategoryId = categoryId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void UpdateDetails(
        string name,
        Guid categoryId,
        string bio,
        string logoUrl,
        string coverImageUrl,
        string? socialLinks,
         string? websiteUrl)
    {
        Name = name;
        CategoryId = categoryId;
        Bio = bio;
        WebsiteUrl = websiteUrl;
        LogoUrl = logoUrl;
        CoverImageUrl = coverImageUrl;
        SocialLinks = socialLinks;
    }
}


