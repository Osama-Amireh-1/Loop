using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Loop.Application.Offers.Contract;

public class GetOffersByShopCategoryResponse
{
    [Required]
    public string CategoryName { get; set; }
    [Required]
    public List<OfferItem> Offers { get; set; }
}

public class OfferItem
{
    [Required]
    public string ShopName { get; set; }
    [Required]
    public string OfferDescription { get; set; }
    [Required]
    public string CoverImageUrl { get; set; }
}
