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

    public static readonly Error DuplicateUpload = new(
        "Receipt.Duplicate",
        "This receipt image has already been uploaded.",
        ErrorType.Failure);

    public static Error ImageHashAlreadyExists(string hash) => Error.Conflict(
        "Receipts.ImageHashAlreadyExists",
        $"A receipt image with the hash '{hash}' already exists.");
}



