using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Loop.Application.Receipts.Contract;
using Loop.SharedKernel;

namespace Loop.Application.Abstractions.Ocr;

public interface IReceiptOcrProvider
{
    Task<Result<ReceiptOcrResult>> ProcessAsync(Stream imageStream, string contentType = "image/jpeg", CancellationToken cancellationToken = default);
}
