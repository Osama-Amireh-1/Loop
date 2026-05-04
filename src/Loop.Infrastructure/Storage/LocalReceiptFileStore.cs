using Loop.Application.Abstractions.Storage;

namespace Loop.Infrastructure.Storage;

internal sealed class LocalReceiptFileStore : IReceiptFileStore
{
    public async Task<string> SaveAsync(Guid mallId, Guid receiptId, string fileName, byte[] content, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";

        var relativePath = Path.Combine("receipts", mallId.ToString("N"), $"{receiptId:N}{extension}");
        var fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, content, cancellationToken);

        return relativePath.Replace(Path.DirectorySeparatorChar, '/');
    }
}
