using Loop.SharedKernel;

namespace Loop.Domain.Receipts;

public static class ReceiptErrors
{
    public static Error NotFound(Guid receiptId) => Error.NotFound(
        "Receipts.NotFound",
        $"The receipt with the Id = '{receiptId}' was not found");

    public static Error ShopNotMatched(string? merchantName) => Error.NotFound(
        "Receipts.ShopNotMatched",
        $"No shop matched the receipt merchant '{merchantName ?? "unknown"}'");

    public static readonly Error AlreadyProcessed = Error.Failure(
        "Receipts.AlreadyProcessed",
        "The receipt has already been approved or rejected");

    public static readonly Error InvalidAmount = Error.Failure(
        "Receipts.InvalidAmount",
        "The receipt amount must be a positive value");
}


