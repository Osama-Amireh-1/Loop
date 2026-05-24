namespace Loop.Web.Api.Controllers;

public sealed class ConfirmOfferRedemptionRequest
{
    public required Guid QrId { get; set; }
}
