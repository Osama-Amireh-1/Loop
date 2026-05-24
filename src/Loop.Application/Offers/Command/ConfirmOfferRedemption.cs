using System.Text.Json;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.Audit;
using Loop.Domain.Offers;
using Loop.Domain.Offers.Specifications;
using Loop.Domain.QRCode;
using Loop.Domain.QRCode.Specifications;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Offers.Command;

public static class ConfirmOfferRedemption
{
    public sealed record Command(Guid QrId) : ICommand<bool>;

    public sealed class Handler(
        IRepository<QrCode> qrCodeRepo,
        IRepository<Offer> offerRepo,
        IRepository<OfferRedemption> redemptionRepo,
        IRepository<User> userRepo,
        IRepository<AuditLog> auditLogRepo,
        IReadOnlyRepository<OfferRedemption> redemptionReadRepo,
        IDateTimeProvider dateTimeProvider,
        IOfferRedemptionQrTokenProvider offerRedemptionQrTokenProvider,
        IShopAdminContext shopAdminContext)
        : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            // Validate QR code
            var qrCode = await qrCodeRepo
                .Find(new QrCodeByPKSpecification(request.QrId))
                .FirstOrDefaultAsync(cancellationToken);

            if (qrCode is null)
            {
                return Result.Failure<bool>(QrCodeErrors.NotFound(request.QrId));
            }

            var token = JsonSerializer.Deserialize<string>(qrCode.QrCodeData) ?? qrCode.QrCodeData;
            OfferRedemptionQrTokenPayload? payload =
                await offerRedemptionQrTokenProvider.ValidateAndGetPayloadAsync(token);

            if (payload is null || qrCode.UserId != payload.UserId)
            {
                return Result.Failure<bool>(QrCodeErrors.InvalidPayload);
            }

            DateTime utcNow = dateTimeProvider.UtcNow;

            if (payload.ExpiresAtUtc <= utcNow || qrCode.IsExpired(utcNow))
            {
                return Result.Failure<bool>(QrCodeErrors.Expired);
            }

            // Check if already redeemed
            bool alreadyRedeemed = await redemptionReadRepo
                .Find(new UserOfferRedemptionSpecification(payload.UserId, payload.OfferId))
                .AnyAsync(cancellationToken);

            if (alreadyRedeemed)
            {
                return Result.Failure<bool>(
                    OfferErrors.UserAlreadyRedeemed(payload.UserId, payload.OfferId));
            }

            var offer = await offerRepo
                .Find(new OfferByIdWithRedemptionsSpecification(payload.OfferId))
                .FirstOrDefaultAsync(cancellationToken);

            if (offer is null)
            {
                return Result.Failure<bool>(OfferErrors.NotFound(payload.OfferId));
            }

            // Get user
            var user = await userRepo
                .Find(new UserWithDetailsSpecification(payload.UserId))
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<bool>(UserErrors.NotFound(payload.UserId));
            }

            var redemption = offer.Redeem(payload.UserId, shopAdminContext.ShopId, request.QrId);
            redemption.Confirm();
            await redemptionRepo.AddAsync(redemption);

            int pointsAwarded = 0;
            if (offer.RewardType == RewardType.Points)
            {
                if (!TryParsePointsRewardValue(offer.RewardValue, out int pointsToAdd) || pointsToAdd <= 0)
                {
                    return Result.Failure<bool>(OfferErrors.InvalidRewardValue);
                }

                var creditResult = user.CreditPoints(pointsToAdd);
                if (creditResult.IsFailure)
                {
                    return Result.Failure<bool>(creditResult.Error);
                }

                pointsAwarded = pointsToAdd;
            }

            qrCode.Invalidate(utcNow);

            var auditLog = AuditLog.Record(
                actionType: "OfferRedeemed",
                userId: payload.UserId,
                shopId: shopAdminContext.ShopId,
                shopAdminId: shopAdminContext.ShopAdminId,
                adminType: AdminType.ShopAdmin,
                points: pointsAwarded > 0 ? pointsAwarded : null,
                metadata: JsonSerializer.Serialize(new
                {
                    redemptionId = redemption.RedemptionId,
                    offerId = offer.OfferId,
                    qrId = qrCode.QrId,
                    offerName = offer.Name,
                    rewardType = offer.RewardType.ToString(),
                    rewardValue = offer.RewardValue,
                    points = pointsAwarded,
                    redeemedAtUtc = utcNow
                }));

            await auditLogRepo.AddAsync(auditLog);

            return Result.Success(true);
        }

        private static bool TryParsePointsRewardValue(string rewardValue, out int points)
        {
            if (int.TryParse(rewardValue, out points))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(rewardValue))
            {
                points = 0;
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(rewardValue);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Number && root.TryGetInt32(out points))
                {
                    return true;
                }

                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("value", out var valueElement) && valueElement.TryGetInt32(out points))
                    {
                        return true;
                    }

                    if (root.TryGetProperty("points", out var pointsElement) && pointsElement.TryGetInt32(out points))
                    {
                        return true;
                    }
                }
            }
            catch (JsonException)
            {
                points = 0;
                return false;
            }

            points = 0;
            return false;
        }
    }
}
