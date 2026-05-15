using System.ComponentModel.DataAnnotations;

namespace Loop.Application.Shops.Contract;

public class SocialLinkDto
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Link { get; set; }
}
