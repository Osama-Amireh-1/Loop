using Domain.Common;
using Domain.Shops;
using Domain.Users;
using Loop.SharedKernel;

namespace Domain.Receipts;

public class Receipt : AggregateRoot
{
    public Guid ReceiptId { get; private set; }
    public string ReceiptPath { get; private set; }
    public Guid ShopId { get; private set; }
    public Guid UserId { get; private set; }
    public Money Amount { get; private set; }
    public string ReceiptDetails { get; private set; } 
    public ReceiptStatus Status { get; private set; }
    public User User { get; private set; }
    public Shop Shop { get; private set; }

    private Receipt() { }

    public static Receipt Upload(
        Guid userId,
        Guid shopId,
        string receiptPath,
        Money amount,
        string receiptDetails)
        => new()
        {
            ReceiptId = Guid.NewGuid(),
            UserId = userId,
            ShopId = shopId,
            ReceiptPath = receiptPath,
            Amount = amount,
            ReceiptDetails = receiptDetails,
            Status = ReceiptStatus.Pending
        };

    public void Approve()
    {
        if (Status != ReceiptStatus.Pending)
            throw new DomainException("Only pending receipts can be approved.");
        Status = ReceiptStatus.Approved;
    }

    public void Reject()
    {
        if (Status != ReceiptStatus.Pending)
            throw new DomainException("Only pending receipts can be rejected.");
        Status = ReceiptStatus.Rejected;
    }
}
