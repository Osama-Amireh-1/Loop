using System.Text.Json;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Abstractions.Ocr;
using Loop.Application.Abstractions.Storage;
using Loop.Application.Interfaces;
using Loop.Application.Receipts.Contract;
using Loop.Application.Receipts.Services;
using Loop.Domain.Audit;
using Loop.Domain.Common;
using Loop.Domain.Configuration;
using Loop.Domain.Configuration.Specifications;
using Loop.Domain.Receipts;
using Loop.Domain.Shops;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Receipts.Command;

public sealed class ProcessReceiptOcr
{
    public sealed record Command(
        Guid MallId,
        string FileName,
        string ContentType,
        byte[] ImageBytes) : ICommand<ReceiptOcrResult>;

    public sealed class Handler(
        IReceiptOcrProvider ocrProvider,
        IMerchantMatcher merchantMatcher,
        IReadOnlyRepository<User> userReadRepo,
        IReadOnlyRepository<SystemConfig> systemConfigReadRepo,
        IRepository<Receipt> receiptRepo,
        IRepository<AuditLog> auditLogRepo,
        IReceiptFileStore receiptFileStore,
        IUserContext userContext) : ICommandHandler<Command, ReceiptOcrResult>
    {
        public async Task<Result<ReceiptOcrResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            await using var imageStream = new MemoryStream(request.ImageBytes);
            var ocrResult = await ocrProvider.ProcessAsync(imageStream, request.ContentType, cancellationToken);
            var matchedResult = await merchantMatcher.MatchAsync(request.MallId, ocrResult, cancellationToken);

            if (matchedResult.MatchedShopId is null)
            {
                return Result.Failure<ReceiptOcrResult>(ReceiptErrors.ShopNotMatched(matchedResult.StoreName ?? matchedResult.MerchantName));
            }

            if (matchedResult.Subtotal is null || matchedResult.Subtotal <= 0)
            {
                return Result.Failure<ReceiptOcrResult>(ReceiptErrors.InvalidAmount);
            }

            var user = await userReadRepo
                        .Find(new UserWithDetailsSpecification(userContext.UserId))
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<ReceiptOcrResult>(UserErrors.NotFound(userContext.UserId));
            }

            var systemConfig = await systemConfigReadRepo
                .Find(new SystemConfigByMallSpecification(request.MallId))
                .FirstOrDefaultAsync(cancellationToken);

            if (systemConfig is null)
            {
                return Result.Failure<ReceiptOcrResult>(SystemConfigErrors.NotFound(request.MallId));
            }

            var receiptId = Guid.NewGuid();
            var receiptPath = await receiptFileStore.SaveAsync(
                request.MallId,
                receiptId,
                request.FileName,
                request.ImageBytes,
                cancellationToken);

            var receipt = Receipt.Upload(
                user.UserId,
                matchedResult.MatchedShopId.Value,
                receiptPath,
                Money.Create(matchedResult.Subtotal.Value),
                JsonSerializer.Serialize(new
                {
                    ocr = matchedResult,
                    mallId = request.MallId
                }),
                receiptId);

            var auditActionType = matchedResult.IsPendingReview ? "ReceiptPendingReview" : "ReceiptProcessed";
            var earnedPoints = matchedResult.IsPendingReview ? 0 : systemConfig.CalculateEarnedPoints(matchedResult.Subtotal.Value);

            if (!matchedResult.IsPendingReview)
            {
                user.CreditPoints(earnedPoints);
                receipt.Approve();
            }

            await receiptRepo.AddAsync(receipt);

            var auditLog = AuditLog.Record(
                actionType: auditActionType,
                userId: user.UserId,
                shopId: matchedResult.MatchedShopId,
                points: earnedPoints,
                metadata: JsonSerializer.Serialize(new
                {
                    receiptId = receipt.ReceiptId,
                    receiptPath,
                    shopId = matchedResult.MatchedShopId,
                    shopName = matchedResult.MatchedShopName,
                    subtotal = matchedResult.Subtotal,
                    currency = matchedResult.Currency,
                    earnedPoints,
                    pendingReview = matchedResult.IsPendingReview,
                    ocrText = matchedResult.RawText,
                    mallId = request.MallId
                }));

            await auditLogRepo.AddAsync(auditLog);

            return Result.Success(matchedResult);
        }
    }
}
