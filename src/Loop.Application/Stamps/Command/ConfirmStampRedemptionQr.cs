using System.Text.Json;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.QRCode;
using Loop.Domain.QRCode.Specifications;
using Loop.Domain.Stamps;
using Loop.Domain.Stamps.Specifications;
using Loop.Domain.Stamps.Specificarions;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Stamps.Command;

public static class ConfirmStampRedemptionQr
{
    public sealed record Command(Guid QrId) : ICommand<bool>;

    public sealed class Handler(
        IRepository<QrCode> qrCodeRepo,
        IRepository<UserStampCard> userStampCardRepo,
        IRepository<StampRedemption> stampRedemptionRepo,
        IReadOnlyRepository<StampRedemption> stampRedemptionReadRepo,
        IDateTimeProvider dateTimeProvider,
        IStampRedemptionQrTokenProvider stampRedemptionQrTokenProvider,
        IShopAdminContext shopAdminContext)
        : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            QrCode? qrCode = await qrCodeRepo
                .Find(new QrCodeByPKSpecification(request.QrId))
                .FirstOrDefaultAsync(cancellationToken);

            if (qrCode is null)
            {
                return Result.Failure<bool>(StampErrors.QrCodeNotFound);
            }

            var token = JsonSerializer.Deserialize<string>(qrCode.QrCodeData) ?? qrCode.QrCodeData;
            StampRedemptionQrTokenPayload? payload = await stampRedemptionQrTokenProvider.ValidateAndGetPayloadAsync(token);

            if (payload is null)
            {
                return Result.Failure<bool>(StampErrors.InvalidQrPayload);
            }

            if (payload.ShopId != shopAdminContext.ShopId)
            {
                return Result.Failure<bool>(StampErrors.InvalidQrPayload);
            }

            DateTime utcNow = dateTimeProvider.UtcNow;

            if (payload.ExpiresAtUtc <= utcNow || qrCode.IsExpired(utcNow))
            {
                return Result.Failure<bool>(StampErrors.QrCodeExpired);
            }

            bool alreadyRedeemed = await stampRedemptionReadRepo
                .Find(new StampRedemptionByQrIdSpecification(qrCode.QrId))
                .AnyAsync(cancellationToken);

            if (alreadyRedeemed)
            {
                return Result.Failure<bool>(StampErrors.QrCodeAlreadyUsed);
            }

            UserStampCard? userStampCard = await userStampCardRepo
                .Find(new ActiveUserStampCardsWithDetailsSpecification(payload.UserId))
                .Where(usc => usc.StampId == payload.StampId)
                .FirstOrDefaultAsync(cancellationToken);

            if (userStampCard is null)
            {
                return Result.Failure<bool>(StampErrors.CardNotFound(payload.UserId, payload.StampId));
            }

            if (!userStampCard.IsCompleted)
            {
                return Result.Failure<bool>(StampErrors.CardNotCompleted);
            }

            qrCode.Invalidate(utcNow);

            var stampRedemption = StampRedemption.Create(
                payload.UserId,
                payload.ShopId,
                payload.StampId,
                qrCode.QrId);

            await stampRedemptionRepo.AddAsync(stampRedemption);

            return Result.Success(true);
        }
    }
}
