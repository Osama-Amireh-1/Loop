using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Loop.Application.Receipts.Contract;

namespace Loop.Application.Abstractions.Ocr;

public interface IReceiptOcrProvider
{
    /// <summary>
    /// Processes the receipt image stream and returns extracted OCR result.
    /// </summary>
    Task<ReceiptOcrResult> ProcessAsync(Stream imageStream, string contentType = "image/jpeg", CancellationToken cancellationToken = default);
}
