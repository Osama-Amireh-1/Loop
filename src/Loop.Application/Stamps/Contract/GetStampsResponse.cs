using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Loop.Application.Stamps.Contract;

public class GetStampsResponse
{

    [Required]
    public string StampName { get; set; }
    [Required]
    public int StampsCounter { get; set; }
    public string? IconUrl { get; set; }
    [Required]
    public int StampsRequired { get; set; }
    [Required]
    public string StampDescription { get; set; }
    [Required]
    public bool IsComplete { get; set; }
}
