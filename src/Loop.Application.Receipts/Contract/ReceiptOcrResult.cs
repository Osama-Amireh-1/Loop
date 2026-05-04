namespace Loop.Application.Receipts.Contract;

public sealed class ReceiptOcrResult
{
    public string? MerchantName { get; init; }
    public string? RawText { get; init; }
}
