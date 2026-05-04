using System;
using System.ComponentModel.DataAnnotations;

namespace Loop.Application.Shops.Contract;

public class GetShopsParams
{
    public Guid? CategoryId { get; set; }

    public string? SearchTerm { get; set; }
}
