using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Loop.Application.Shops.Contract;

public class GetShopsResponse
{
    [Required]
    public Guid ShopId { get; set; }
    [Required]
    public string ShopName { get; set; }
    [Required]
    public string CategoryName { get; set; }
    [Required]
    public string ShopImageUrl { get; set; }

}
