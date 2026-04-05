using SharedKernel;

namespace Domain.Offers;

public static class OfferRedemptionErrors
{
    public static Error NotFound(Guid redemptionId) => Error.NotFound(
        "Offers.RedemptionNotFound",
        $"The offer redemption with the Id = '{redemptionId}' was not found");
}
