using System.Text.Json;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.Audit;
using Loop.Domain.QRCode;
using Loop.Domain.QRCode.Specifications;
using Loop.Domain.Stamps;
using Loop.Domain.Stamps.Specificarions;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Stamps.Command;

public static class ConfirmStampRedemptionQr
{
    public sealed record Command(Guid QrId) : ICommand<bool>;

    public sealed class Handler(
        IRepository<QrCode> qrCodeRepo,
        IRepository<StampRedemption> stampRedemptionRepo,
        IRepository<AuditLog> auditLogRepo,
        IReadOnlyRepository<StampRedemption> stampRedemptionReadRepo,
        IDateTimeProvider dateTimeProvider,
        IStampRedemptionQrTokenProvider stampRedemptionQrTokenProvider,
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
                return Result.Failure<bool>(StampErrors.QrCodeNotFound);
            }

            StampRedemptionQrTokenPayload? payload = await stampRedemptionQrTokenProvider.ValidateAndGetPayloadAsync(qrCode.QrCodeData);

            if (payload is null)
            {
                return Result.Failure<bool>(StampErrors.InvalidQrPayload);
            }

            if (payload.ShopId != shopAdminContext.ShopId || qrCode.ShopId != shopAdminContext.ShopId)
            {
                return Result.Failure<bool>(StampErrors.InvalidQrPayload);
            }

            DateTime utcNow = dateTimeProvider.UtcNow;

            if (payload.ExpiresAtUtc <= utcNow || qrCode.IsExpired(utcNow))
            {
                return Result.Failure<bool>(StampErrors.QrCodeExpired);
            }

            bool alreadyUsed = await stampRedemptionReadRepo
                .Find(new StampRedemptionByQrIdSpecification(qrCode.QrId))
                .AnyAsync(cancellationToken);

            if (alreadyUsed)
            {
                return Result.Failure<bool>(StampErrors.QrCodeAlreadyUsed);
            }

            var redemption = StampRedemption.Create(
                payload.UserId,
                payload.ShopId,
                payload.StampId,
                qrCode.QrId);

            await stampRedemptionRepo.AddAsync(redemption);

            var auditLog = AuditLog.Record(
                actionType: "StampRedeemed",
                userId: payload.UserId,
                shopId: payload.ShopId,
                shopAdminId: shopAdminContext.ShopAdminId,
                adminType: AdminType.ShopAdmin,
                metadata: JsonSerializer.Serialize(new
                {
                    redemptionId = redemption.RedemptionId,
                    redemptionRef = qrCode.QrId,
                    stampId = payload.StampId,
                    shopId = payload.ShopId,
                    userId = payload.UserId,
                    redeemedAtUtc = redemption.CreatedAt
                }));

            await auditLogRepo.AddAsync(auditLog);

            return Result.Success(true);
        }
    }
}
