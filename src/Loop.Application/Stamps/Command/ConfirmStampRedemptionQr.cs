using System.Text.Json;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.Audit;
using Loop.Domain.QRCode;
using Loop.Domain.Stamps;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Stamps.Command;

public static class ConfirmStampRedemptionQr
{
    public sealed record Command(string QrCodeData) : ICommand<bool>;

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
            StampRedemptionQrTokenPayload? payload = await stampRedemptionQrTokenProvider.ValidateAndGetPayloadAsync(request.QrCodeData);

            if (payload is null)
            {
                return Result.Failure<bool>(StampErrors.InvalidQrPayload);
            }

            if (payload.ShopId != shopAdminContext.ShopId)
            {
                return Result.Failure<bool>(StampErrors.InvalidQrPayload);
            }

            var qrCode = await qrCodeRepo.GetAll()
                .FirstOrDefaultAsync(q =>
                    q.QrCodeData == request.QrCodeData &&
                    q.UserId == payload.UserId &&
                    q.ShopId == payload.ShopId,
                    cancellationToken);

            if (qrCode is null)
            {
                return Result.Failure<bool>(StampErrors.QrCodeNotFound);
            }

            DateTime utcNow = dateTimeProvider.UtcNow;

            if (payload.ExpiresAtUtc <= utcNow || qrCode.IsExpired(utcNow))
            {
                return Result.Failure<bool>(StampErrors.QrCodeExpired);
            }

            bool alreadyUsed = await stampRedemptionReadRepo.GetAll()
                .AnyAsync(sr => sr.RedemptionRef == qrCode.QrId, cancellationToken);

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
