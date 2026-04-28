using Loop.SharedKernel;

namespace Loop.Domain.Transactions;

public static class TransactionErrors
{
    public static Error EarnNotFound(Guid earnId) => Error.NotFound(
        "Transactions.EarnNotFound",
        $"The earn transaction with the Id = '{earnId}' was not found");

    public static Error RedeemNotFound(Guid redeemId) => Error.NotFound(
        "Transactions.RedeemNotFound",
        $"The redeem transaction with the Id = '{redeemId}' was not found");

    public static readonly Error InvalidPurchaseAmount = Error.Failure(
        "Transactions.InvalidPurchaseAmount",
        "The purchase amount must be a positive value");

    public static readonly Error InsufficientPoints = Error.Failure(
        "Transactions.InsufficientPoints",
        "The user does not have enough points for this redemption");

    public static readonly Error RedemptionAlreadyProcessed = Error.Failure(
        "Transactions.RedemptionAlreadyProcessed",
        "The redemption has already been verified or cancelled");

    public static readonly Error InvalidVerificationCode = Error.Failure(
        "Transactions.InvalidVerificationCode",
        "The verification code is invalid");

    public static readonly Error BelowMinRedemptionThreshold = Error.Failure(
        "Transactions.BelowMinRedemptionThreshold",
        "The user's points are below the minimum redemption threshold");

    public static readonly Error InvalidRedemptionPoints = Error.Failure(
        "Transactions.InvalidRedemptionPoints",
        "The points to redeem must be greater than zero");

    public static readonly Error InvalidQrPayload = Error.Failure(
        "Transactions.InvalidQrPayload",
        "The redemption QR payload is invalid");

    public static readonly Error QrCodeNotFound = Error.NotFound(
        "Transactions.QrCodeNotFound",
        "The redemption QR code was not found");

    public static readonly Error QrCodeExpired = Error.Failure(
        "Transactions.QrCodeExpired",
        "The redemption QR code has expired");
}




