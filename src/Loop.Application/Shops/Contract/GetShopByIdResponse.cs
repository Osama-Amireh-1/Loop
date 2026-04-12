using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Loop.Application.Shops.Contract;

public class GetShopByIdResponse
{
    [Required]
    public string ShopName { get; set; }
    [Required]
    public string CategoryName { get; set; }
    [Required]
    public string LogoUrl { get; set; }
    [Required]
    public string CoverUrl { get; set; }

    public string? WebsiteLink { get; set; }

    public List<string> SocialLinks { get; set; }
}
