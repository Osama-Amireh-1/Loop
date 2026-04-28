using System.Text.Json;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.Audit;
using Loop.Domain.Configuration;
using Loop.Domain.Configuration.Specifications;
using Loop.Domain.QRCode;
using Loop.Domain.Shops;
using Loop.Domain.Transactions;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Users.Command;

public static class ConfirmPointsRedemptionQr
{
    public sealed record Command(string QrCodeData) : ICommand<bool>;

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
            PointsRedemptionQrTokenPayload? payload = await pointsRedemptionQrTokenProvider.ValidateAndGetPayloadAsync(request.QrCodeData);

            if (payload is null)
            {
                return Result.Failure<bool>(TransactionErrors.InvalidQrPayload);
            }

            var qrCode = await qrCodeRepo.GetAll()
                .FirstOrDefaultAsync(q => q.QrCodeData == request.QrCodeData && q.UserId == payload.UserId, cancellationToken);

            if (qrCode is null)
            {
                return Result.Failure<bool>(TransactionErrors.QrCodeNotFound);
            }

            DateTime utcNow = dateTimeProvider.UtcNow;

            if (payload.ExpiresAtUtc <= utcNow || qrCode.IsExpired(utcNow))
            {
                return Result.Failure<bool>(TransactionErrors.QrCodeExpired);
            }

            var shop = await shopRepo.GetAll()
                .FirstOrDefaultAsync(s => s.ShopId == shopAdminContext.ShopId, cancellationToken);

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

            var redeemTransaction = RedeemTransaction.Initiate(
                payload.UserId,
                shop.ShopId,
                payload.PointsToRedeem,
                systemConfig.CalculateDiscountValue(payload.PointsToRedeem),
                systemConfig.PointsToCurrencyRatio);

            redeemTransaction.Verify();
            user.DebitPoints(payload.PointsToRedeem);
            shop.AddRedeemedPoints(payload.PointsToRedeem);
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
