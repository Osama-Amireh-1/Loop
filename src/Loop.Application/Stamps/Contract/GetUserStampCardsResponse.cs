using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Loop.Application.Stamps.Contract;

public class GetUserStampCardsResponse
{
    [Required]
    public Guid stampId { get; set; }
    [Required]
    public string shopName { get; set; }
    [Required]
    public string stampName { get; set; }
    [Required]
    public int stampsCounter { get; set; }
    public string? iconUrl { get; set; }
    [Required]
    public int stampsRequired { get; set; }
    public string? stampDescription { get; set; }

}
