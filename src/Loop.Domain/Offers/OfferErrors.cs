using SharedKernel;

namespace Domain.Offers;

public static class OfferErrors
{
    public static Error NotFound(Guid offerId) => Error.NotFound(
        "Offers.NotFound",
        $"The offer with the Id = '{offerId}' was not found");

    public static readonly Error OfferInactive = Error.Failure(
        "Offers.OfferInactive",
        "The offer is currently inactive");

    public static readonly Error OfferExpired = Error.Failure(
        "Offers.OfferExpired",
        "The offer has expired or is not yet active");

    public static readonly Error OfferNotStarted = Error.Failure(
        "Offers.OfferNotStarted",
        "The offer has not started yet");
}
