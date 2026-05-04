namespace Loop.Application.Abstractions.Storage;

public interface IReceiptFileStore
{
    Task<string> SaveAsync(Guid mallId, Guid receiptId, string fileName, byte[] content, CancellationToken cancellationToken = default);
}
