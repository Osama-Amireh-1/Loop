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
}


