using Loop.Application.Receipts.Contract;

namespace Loop.Application.Receipts.Services;

public interface IMerchantMatcher
{
    Task<ReceiptOcrResult> MatchAsync(Guid mallId, ReceiptOcrResult ocrResult, CancellationToken cancellationToken = default);
}
