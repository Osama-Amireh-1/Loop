using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Stamps.Contract;
using Loop.Domain.QRCode;
using Loop.Domain.QRCode.Specifications;
using Loop.Domain.Stamps;
using Loop.Domain.Stamps.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Stamps.Command;

public static class ScanStampCollectQr
{
    public sealed record Command(Guid QrId) : ICommand<ScanStampCollectQrResponse>;

    public sealed class Handler(
        IRepository<QrCode> qrCodeRepo,
        IRepository<UserStampCard> userStampCardRepo,
        IRepository<StampTransaction> stampTransactionRepo,
        IReadOnlyRepository<StampTransaction> stampTransactionReadRepo,
        IReadOnlyRepository<Stamp> stampReadRepo,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IStampCollectionQrTokenProvider stampCollectionQrTokenProvider)
        : ICommandHandler<Command, ScanStampCollectQrResponse>
    {
        public async Task<Result<ScanStampCollectQrResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            QrCode? qrCode = await qrCodeRepo
                .Find(new QrCodeByPKSpecification(request.QrId))
                .FirstOrDefaultAsync(cancellationToken);

            if (qrCode is null)
            {
                return Result.Failure<ScanStampCollectQrResponse>(StampErrors.QrCodeNotFound);
            }

            StampCollectionQrTokenPayload? payload = await stampCollectionQrTokenProvider.ValidateAndGetPayloadAsync(qrCode.QrCodeData);

            if (payload is null)
            {
                return Result.Failure<ScanStampCollectQrResponse>(StampErrors.InvalidQrPayload);
            }

            DateTime utcNow = dateTimeProvider.UtcNow;

            if (payload.ExpiresAtUtc <= utcNow || qrCode.IsExpired(utcNow))
            {
                return Result.Failure<ScanStampCollectQrResponse>(StampErrors.QrCodeExpired);
            }

            bool alreadyUsed = await stampTransactionReadRepo
                .Find(new StampTransactionByQrIdSpecification(qrCode.QrId))
                .AnyAsync(cancellationToken);

            if (alreadyUsed)
            {
                return Result.Failure<ScanStampCollectQrResponse>(StampErrors.QrCodeAlreadyUsed);
            }

            var stamp = await stampReadRepo
                .Find(new ActiveStampsByShopSpecification(payload.ShopId))
                .Where(s => s.StampId == payload.StampId)
                .Select(s => new { s.StampId, s.ShopId, s.StampsRequired })
                .FirstOrDefaultAsync(cancellationToken);

            if (stamp is null)
            {
                return Result.Failure<ScanStampCollectQrResponse>(StampErrors.NotFound(payload.StampId));
            }

            UserStampCard? userStampCard = await userStampCardRepo
                .Find(new ActiveUserStampCardsWithDetailsSpecification(userContext.UserId))
                .Where(usc => usc.StampId == stamp.StampId)
                .FirstOrDefaultAsync(cancellationToken);

            if (userStampCard is null)
            {
                userStampCard = UserStampCard.Open(userContext.UserId, stamp.StampId);
                await userStampCardRepo.AddAsync(userStampCard);
            }

            if (userStampCard.IsCompleted)
            {
                return Result.Failure<ScanStampCollectQrResponse>(StampErrors.CardAlreadyCompleted);
            }

            userStampCard.CollectStamp(stamp.StampsRequired, payload.StampsCount);
            qrCode.Invalidate(utcNow);

            var stampTransaction = StampTransaction.RecordCollect(
                userContext.UserId,
                stamp.ShopId,
                stamp.StampId,
                payload.StampsCount,
                qrCode.QrId);

            await stampTransactionRepo.AddAsync(stampTransaction);

            return Result.Success(new ScanStampCollectQrResponse
            {
                StampId = userStampCard.StampId,
                StampsCounter = userStampCard.StampsCounter,
                IsCompleted = userStampCard.IsCompleted
            });
        }
    }
}
