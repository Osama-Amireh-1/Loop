namespace Loop.Application.Offers.Command;

public sealed class GenerateOfferRedemptionQrResponse
{
    public Guid QrId { get; set; }
    public Guid OfferId { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}
