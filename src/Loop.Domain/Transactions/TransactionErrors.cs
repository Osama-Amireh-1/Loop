using Loop.SharedKernel;

namespace Loop.Domain.Transactions;

public static class TransactionErrors
{
    public static Error EarnNotFound(Guid earnId) => Error.NotFound(
        "Transactions.EarnNotFound",
        $"No earn transaction was found for Id '{earnId}'.");

    public static Error RedeemNotFound(Guid redeemId) => Error.NotFound(
        "Transactions.RedeemNotFound",
        $"No redeem transaction was found for Id '{redeemId}'.");

    public static readonly Error InvalidPurchaseAmount = Error.Failure(
        "Transactions.InvalidPurchaseAmount",
        "The purchase amount must be greater than zero.");

    public static readonly Error InsufficientPoints = Error.Failure(
        "Transactions.InsufficientPoints",
        "The user does not have enough points for this redemption.");

    public static readonly Error RedemptionAlreadyProcessed = Error.Failure(
        "Transactions.RedemptionAlreadyProcessed",
        "This redemption has already been verified or cancelled.");

    public static readonly Error InvalidVerificationCode = Error.Failure(
        "Transactions.InvalidVerificationCode",
        "The verification code is invalid.");

    public static readonly Error BelowMinRedemptionThreshold = Error.Failure(
        "Transactions.BelowMinRedemptionThreshold",
        "The user's total points are below the minimum redemption threshold.");

    public static readonly Error InvalidRedemptionPoints = Error.Failure(
        "Transactions.InvalidRedemptionPoints",
        "The number of points to redeem must be greater than zero.");

    public static readonly Error InvalidQrPayload = Error.Failure(
        "Transactions.InvalidQrPayload",
        "The redemption QR payload is invalid.");

    public static readonly Error QrCodeNotFound = Error.NotFound(
        "Transactions.QrCodeNotFound",
        "The redemption QR code was not found.");

    public static readonly Error QrCodeExpired = Error.Failure(
        "Transactions.QrCodeExpired",
        "The redemption QR code has expired.");
}




