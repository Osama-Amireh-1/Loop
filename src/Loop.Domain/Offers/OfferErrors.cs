using Loop.SharedKernel;

namespace Loop.Domain.Offers;

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

    public static readonly Error InvalidRewardValue = Error.Failure(
        "Offers.InvalidRewardValue",
        "The offer's reward value is invalid");

    public static Error UserAlreadyRedeemed(Guid userId, Guid offerId) => Error.Failure(
        "Offers.UserAlreadyRedeemed",
        $"User {userId} has already redeemed offer {offerId}");

    public static readonly Error QrCodeAlreadyUsed = Error.Failure(
        "Offers.QrCodeAlreadyUsed",
        "This QR code has already been used");
}


