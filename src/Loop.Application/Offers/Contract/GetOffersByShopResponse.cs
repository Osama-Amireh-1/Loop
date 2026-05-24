using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Loop.Application.Offers.Contract;

public class GetOffersByShopResponse
{
    [Required]
    public Guid OfferId { get; set; }

    [Required]
    public string OfferDescription { get; set; }
    [Required]
    public string CoverImageUrl { get; set; }  
}
