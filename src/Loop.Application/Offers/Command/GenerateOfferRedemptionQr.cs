using System.Security.Cryptography;
using System.Text.Json;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.Offers;
using Loop.Domain.Offers.Specifications;
using Loop.Domain.QRCode;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Offers.Command;

public static class GenerateOfferRedemptionQr
{
    public sealed record Command(Guid OfferId) : ICommand<GenerateOfferRedemptionQrResponse>;

    public sealed class Handler(
        IRepository<QrCode> qrCodeRepo,
        IReadOnlyRepository<Offer> offerReadRepo,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IOfferRedemptionQrTokenProvider offerRedemptionQrTokenProvider)
        : ICommandHandler<Command, GenerateOfferRedemptionQrResponse>
    {
        public async Task<Result<GenerateOfferRedemptionQrResponse>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            var offer = await offerReadRepo
                .Find(new OfferByIdWithRedemptionsSpecification(request.OfferId))
                .FirstOrDefaultAsync(cancellationToken);

            if (offer is null)
            {
                return Result.Failure<GenerateOfferRedemptionQrResponse>(
                    OfferErrors.NotFound(request.OfferId));
            }

            DateTime utcNow = dateTimeProvider.UtcNow;

            DateTime expiresAtUtc = utcNow.AddMinutes(2);
            string tokenId = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            
            string qrCodeData = offerRedemptionQrTokenProvider.CreateToken(
                new OfferRedemptionQrTokenPayload(
                    tokenId,
                    request.OfferId,
                    userContext.UserId,
                    expiresAtUtc));

            var qrCode = QrCode.Create(
                userContext.UserId,
                null,
                JsonSerializer.Serialize(qrCodeData),
                expiresAtUtc);

            await qrCodeRepo.AddAsync(qrCode);

            return Result.Success(new GenerateOfferRedemptionQrResponse
            {
                QrId = qrCode.QrId,
                OfferId = request.OfferId,
                ExpiresAtUtc = qrCode.ExpiresAt
            });
        }
    }
}
