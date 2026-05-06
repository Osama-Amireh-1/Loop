namespace Loop.Application.Receipts.Contract;

public sealed class ReceiptOcrResult
{
    public string? StoreName { get; init; }
    public string? MerchantName { get; init; }
    public List<ReceiptLineItem> Items { get; init; } = [];
    public decimal? Subtotal { get; init; }
    public string? Currency { get; init; }
    public bool IsPendingReview { get; init; }
    public string? RawText { get; init; }

    public Guid? MatchedShopId { get; init; }
    public string? MatchedShopName { get; init; }
    public double? MatchScore { get; init; }
}
