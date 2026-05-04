namespace Loop.Application.Receipts.Contract;

public sealed class ReceiptLineItem
{
    public string? Name { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
}
