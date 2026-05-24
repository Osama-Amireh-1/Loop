using System.ComponentModel.DataAnnotations;

namespace Loop.Application.Offers.Contract;

public class GetOfferByIdResponse
{
    [Required]
    public Guid OfferId { get; set; }

    [Required]
    public string OfferName { get; set; }

    [Required]
    public string OfferDescription { get; set; }

    [Required]
    public string OfferImageUrl { get; set; }

    [Required]
    public string RewardType { get; set; }

    [Required]
    public decimal RewardValue { get; set; }

    [Required]
    public Guid ShopId { get; set; }

    [Required]
    public string ShopName { get; set; }

    [Required]
    public string CoverImageUrl { get; set; }
}
