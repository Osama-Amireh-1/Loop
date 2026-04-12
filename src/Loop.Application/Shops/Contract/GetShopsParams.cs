using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Loop.Application.Shops.Contract;

public class GetShopsParams
{
    [Required]
   public Guid MallId { get; set; }

    public Guid? CategoryId { get; set; }

    public string? SearchTerm { get; set; }
}
