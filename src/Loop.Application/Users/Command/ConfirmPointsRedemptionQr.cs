using System.Text.Json;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.Audit;
using Loop.Domain.Configuration;
using Loop.Domain.Configuration.Specifications;
using Loop.Domain.QRCode;
using Loop.Domain.QRCode.Specifications;
using Loop.Domain.Shops;
using Loop.Domain.Shops.Specificarions;
using Loop.Domain.Transactions;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Users.Command;

public static class ConfirmPointsRedemptionQr
{
    public sealed record Command(Guid QrId) : ICommand<bool>;

    public sealed class Handler(
        IRepository<QrCode> qrCodeRepo,
        IRepository<RedeemTransaction> redeemTransactionRepo,
        IRepository<User> userRepo,
        IRepository<AuditLog> auditLogRepo,
        IRepository<Shop> shopRepo,
        IReadOnlyRepository<SystemConfig> systemConfigReadRepo,
        IDateTimeProvider dateTimeProvider,
        IPointsRedemptionQrTokenProvider pointsRedemptionQrTokenProvider,
        IShopAdminContext shopAdminContext)
        : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var qrCode = await qrCodeRepo
                .Find(new QrCodeByPKSpecification(request.QrId))
                .FirstOrDefaultAsync(cancellationToken);

            if (qrCode is null)
            {
                return Result.Failure<bool>(TransactionErrors.QrCodeNotFound);
            }

            var token = JsonSerializer.Deserialize<string>(qrCode.QrCodeData) ?? qrCode.QrCodeData;
            PointsRedemptionQrTokenPayload? payload = await pointsRedemptionQrTokenProvider.ValidateAndGetPayloadAsync(token);

            if (payload is null || qrCode.UserId != payload.UserId)
            {
                return Result.Failure<bool>(TransactionErrors.InvalidQrPayload);
            }

            DateTime utcNow = dateTimeProvider.UtcNow;
            if (payload.ExpiresAtUtc <= utcNow || qrCode.IsExpired(utcNow))
            {
                return Result.Failure<bool>(TransactionErrors.QrCodeExpired);
            }

            var shop = await shopRepo
                .Find(new ShopByIdSpecification(shopAdminContext.ShopId))
                .FirstOrDefaultAsync(cancellationToken);

            if (shop is null)
            {
                return Result.Failure<bool>(ShopErrors.NotFound(shopAdminContext.ShopId));
            }

            var systemConfig = await systemConfigReadRepo
                .Find(new SystemConfigByMallSpecification(shop.MallId))
                .FirstOrDefaultAsync(cancellationToken);

            if (systemConfig is null)
            {
                return Result.Failure<bool>(SystemConfigErrors.NotFound(shop.MallId));
            }

            User? user = await userRepo.Find(new UserByPKSpecification(payload.UserId))
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<bool>(UserErrors.NotFound(payload.UserId));
            }

            if (user.PointsBalance.TotalPoints < systemConfig.MinRedemptionThreshold)
            {
                return Result.Failure<bool>(TransactionErrors.BelowMinRedemptionThreshold);
            }

            if (!user.HasEnoughPoints(payload.PointsToRedeem))
            {
                return Result.Failure<bool>(TransactionErrors.InsufficientPoints);
            }

            var discountResult = systemConfig.CalculateDiscountValue(payload.PointsToRedeem);
            if (discountResult.IsFailure)
            {
                return Result.Failure<bool>(discountResult.Error);
            }

            var redeemTransaction = RedeemTransaction.Initiate(
                payload.UserId,
                shop.ShopId,
                payload.PointsToRedeem,
                discountResult.Value,
                systemConfig.PointsToCurrencyRatio);

            var verifyResult = redeemTransaction.Verify();
            if (verifyResult.IsFailure)
            {
                return Result.Failure<bool>(verifyResult.Error);
            }

            var debitResult = user.DebitPoints(payload.PointsToRedeem);
            if (debitResult.IsFailure)
            {
                return Result.Failure<bool>(debitResult.Error);
            }

            var addPointsResult = shop.PointsWallet.AddPoints(payload.PointsToRedeem);
            if (addPointsResult.IsFailure)
            {
                return Result.Failure<bool>(addPointsResult.Error);
            }

            qrCode.Invalidate(utcNow);
            await redeemTransactionRepo.AddAsync(redeemTransaction);

            var auditLog = AuditLog.Record(
                actionType: "PointsRedeemed",
                userId: payload.UserId,
                shopId: shop.ShopId,
                shopAdminId: shopAdminContext.ShopAdminId,
                adminType: AdminType.ShopAdmin,
                points: payload.PointsToRedeem,
                metadata: JsonSerializer.Serialize(new
                {
                    redeemId = redeemTransaction.RedeemId,
                    qrId = qrCode.QrId,
                    pointsUsed = payload.PointsToRedeem,
                    discountValue = redeemTransaction.DiscountValue.Amount,
                    appliedRatio = redeemTransaction.AppliedPointsToCurrencyRatio,
                    redeemedAtUtc = redeemTransaction.CompletedAt,
                    shopPointsReceived = shop.PointsWallet.PointsReceived
                }));

            await auditLogRepo.AddAsync(auditLog);

            return Result.Success(true);
        }
    }
}
