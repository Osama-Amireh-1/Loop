using Loop.Domain.Common;
using Loop.Domain.Receipts;
using Loop.SharedKernel;
using Shouldly;

namespace Loop.ArchitectureTests.Domain.Receipts;

public class ReceiptTests
{
    [Fact]
    public void Upload_ShouldCreatePendingReceipt_WithGeneratedIdAndDefaultImageHash()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var amount = Money.Create(12.75m).Value;

        var receipt = Receipt.Upload(userId, shopId, "/receipts/1.jpg", amount, "Raw OCR details");

        receipt.ReceiptId.ShouldNotBe(Guid.Empty);
        receipt.UserId.ShouldBe(userId);
        receipt.ShopId.ShouldBe(shopId);
        receipt.ReceiptPath.ShouldBe("/receipts/1.jpg");
        receipt.Amount.ShouldBe(amount);
        receipt.ReceiptDetails.ShouldBe("Raw OCR details");
        receipt.Status.ShouldBe(ReceiptStatus.Pending);
        receipt.ImageHash.ShouldBe(string.Empty);
    }

    [Fact]
    public void Upload_ShouldUseProvidedReceiptIdAndImageHash_WhenProvided()
    {
        var receiptId = Guid.NewGuid();
        var amount = Money.Create(20m).Value;

        var receipt = Receipt.Upload(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "/receipts/2.jpg",
            amount,
            "Raw OCR details",
            receiptId,
            "hash-123");

        receipt.ReceiptId.ShouldBe(receiptId);
        receipt.ImageHash.ShouldBe("hash-123");
    }

    [Fact]
    public void Approve_ShouldSetStatusToApproved_WhenReceiptIsPending()
    {
        var receipt = CreatePendingReceipt();

        receipt.Approve();

        receipt.Status.ShouldBe(ReceiptStatus.Approved);
    }

    [Fact]
    public void Approve_ShouldThrowDomainException_WhenReceiptIsNotPending()
    {
        var receipt = CreatePendingReceipt();
        receipt.Reject();

        var exception = Should.Throw<DomainException>(() => receipt.Approve());

        exception.Message.ShouldBe("Only pending receipts can be approved.");
    }

    [Fact]
    public void Reject_ShouldSetStatusToRejected_WhenReceiptIsPending()
    {
        var receipt = CreatePendingReceipt();

        receipt.Reject();

        receipt.Status.ShouldBe(ReceiptStatus.Rejected);
    }

    [Fact]
    public void Reject_ShouldThrowDomainException_WhenReceiptIsNotPending()
    {
        var receipt = CreatePendingReceipt();
        receipt.Approve();

        var exception = Should.Throw<DomainException>(() => receipt.Reject());

        exception.Message.ShouldBe("Only pending receipts can be rejected.");
    }

    private static Receipt CreatePendingReceipt() =>
        Receipt.Upload(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "/receipts/pending.jpg",
            Money.Create(10m).Value,
            "Details");
}
