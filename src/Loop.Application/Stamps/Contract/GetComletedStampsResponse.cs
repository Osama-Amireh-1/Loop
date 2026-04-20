using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Loop.Application.Stamps.Contract;

public class GetComletedStampsResponse
{
    [Required]
    public Guid StampId { get; set; }
    [Required]
    public string ShopName { get; set; }
    public string? IconUrl { get; set; }
    [Required]
    public string StampDescription { get; set; }
}
