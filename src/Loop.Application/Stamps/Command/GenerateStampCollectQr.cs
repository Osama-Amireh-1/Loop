using System.Security.Cryptography;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Stamps.Contract;
using Loop.Domain.QRCode;
using Loop.Domain.Stamps;
using Loop.Domain.Stamps.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Stamps.Command;

public static class GenerateStampCollectQr
{
    public sealed record Command(Guid StampId, int StampsCount = 1) : ICommand<GenerateStampCollectQrResponse>;

    public sealed class Handler(
        IRepository<QrCode> qrCodeRepo,
        IReadOnlyRepository<Stamp> stampReadRepo,
        IShopAdminContext shopAdminContext,
        IDateTimeProvider dateTimeProvider,
        IStampCollectionQrTokenProvider stampCollectionQrTokenProvider)
        : ICommandHandler<Command, GenerateStampCollectQrResponse>
    {
        public async Task<Result<GenerateStampCollectQrResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.StampsCount <= 0)
            {
                return Result.Failure<GenerateStampCollectQrResponse>(StampErrors.InvalidQrPayload);
            }

            bool stampExists = await stampReadRepo
                .Find(new ActiveStampsByShopSpecification(shopAdminContext.ShopId))
                .AnyAsync(s => s.StampId == request.StampId, cancellationToken);

            if (!stampExists)
            {
                return Result.Failure<GenerateStampCollectQrResponse>(StampErrors.NotFound(request.StampId));
            }

            DateTime expiresAtUtc = dateTimeProvider.UtcNow.AddMinutes(2);
            string tokenId = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            string qrCodeData = stampCollectionQrTokenProvider.CreateToken(
                new StampCollectionQrTokenPayload(
                    tokenId,
                    request.StampId,
                    shopAdminContext.ShopId,
                    request.StampsCount,
                    expiresAtUtc));

            var qrCode = QrCode.Create(null, shopAdminContext.ShopId, qrCodeData, expiresAtUtc);
            await qrCodeRepo.AddAsync(qrCode);

            return Result.Success(new GenerateStampCollectQrResponse
            {
                QrId = qrCode.QrId,
                ExpiresAtUtc = qrCode.ExpiresAt
            });
        }
    }
}
