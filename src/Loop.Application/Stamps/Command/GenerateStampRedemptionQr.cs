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

public static class GenerateStampRedemptionQr
{
    public sealed record Command(Guid StampId) : ICommand<GenerateStampRedemptionQrResponse>;

    public sealed class Handler(
        IRepository<QrCode> qrCodeRepo,
        IReadOnlyRepository<UserStampCard> userStampCardReadRepo,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IStampRedemptionQrTokenProvider stampRedemptionQrTokenProvider)
        : ICommandHandler<Command, GenerateStampRedemptionQrResponse>
    {
        public async Task<Result<GenerateStampRedemptionQrResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userStampCard = await userStampCardReadRepo
                .Find(new ActiveUserStampCardsWithDetailsSpecification(userContext.UserId))
                .Where(usc => usc.StampId == request.StampId)
                .Select(usc => new { usc.StampId, usc.IsCompleted, usc.Stamp.ShopId })
                .FirstOrDefaultAsync(cancellationToken);

            if (userStampCard is null)
            {
                return Result.Failure<GenerateStampRedemptionQrResponse>(StampErrors.CardNotFound(userContext.UserId, request.StampId));
            }

            if (!userStampCard.IsCompleted)
            {
                return Result.Failure<GenerateStampRedemptionQrResponse>(StampErrors.CardNotCompleted);
            }

            DateTime expiresAtUtc = dateTimeProvider.UtcNow.AddMinutes(2);

            string tokenId = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            string qrCodeData = stampRedemptionQrTokenProvider.CreateToken(
                new StampRedemptionQrTokenPayload(
                    tokenId,
                    userStampCard.StampId,
                    userContext.UserId,
                    userStampCard.ShopId,
                    expiresAtUtc));

            var qrCode = QrCode.Create(userContext.UserId, userStampCard.ShopId, qrCodeData, expiresAtUtc);
            await qrCodeRepo.AddAsync(qrCode);

            return Result.Success(new GenerateStampRedemptionQrResponse
            {
                QrId = qrCode.QrId,
                QrCodeData = qrCode.QrCodeData,
                ExpiresAtUtc = qrCode.ExpiresAt
            });
        }
    }
}
