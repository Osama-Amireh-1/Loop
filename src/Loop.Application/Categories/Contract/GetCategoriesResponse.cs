using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Loop.Application.Categories.Contract;

public class GetCategoriesResponse
{
    [Required]
    public Guid CategoryId { get; set; }
    [Required]
    public string CategoryName { get; set; }
}
