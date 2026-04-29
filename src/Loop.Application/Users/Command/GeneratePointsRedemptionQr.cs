using System.Security.Cryptography;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Users.Contract;
using Loop.Domain.QRCode;
using Loop.Domain.Transactions;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Users.Command;

public static class GeneratePointsRedemptionQr
{
    public sealed record Command(int PointsToRedeem) : ICommand<GeneratePointsRedemptionQrResponse>;

    public sealed class Handler(
        IRepository<QrCode> qrCodeRepo,
        IReadOnlyRepository<User> userReadRepo,
        IUserContext userContext,
        IDateTimeProvider dateTimeProvider,
        IPointsRedemptionQrTokenProvider pointsRedemptionQrTokenProvider)
        : ICommandHandler<Command, GeneratePointsRedemptionQrResponse>
    {
        public async Task<Result<GeneratePointsRedemptionQrResponse>> Handle(Command request, CancellationToken cancellationToken)
        {
            if (request.PointsToRedeem <= 0)
            {
                return Result.Failure<GeneratePointsRedemptionQrResponse>(TransactionErrors.InvalidRedemptionPoints);
            }

            int? userPoints = await userReadRepo
                .Find(new UserByPKSpecification(userContext.UserId))
                .Select(u => (int?)u.PointsBalance.TotalPoints)
                .FirstOrDefaultAsync(cancellationToken);

            if (userPoints is null)
            {
                return Result.Failure<GeneratePointsRedemptionQrResponse>(UserErrors.NotFound(userContext.UserId));
            }

            if (request.PointsToRedeem > userPoints.Value)
            {
                return Result.Failure<GeneratePointsRedemptionQrResponse>(TransactionErrors.InsufficientPoints);
            }

            DateTime expiresAtUtc = dateTimeProvider.UtcNow.AddMinutes(2);
            string tokenId = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
            string qrCodeData = pointsRedemptionQrTokenProvider.CreateToken(
                new PointsRedemptionQrTokenPayload(
                    tokenId,
                    userContext.UserId,
                    request.PointsToRedeem,
                    expiresAtUtc));

            var qrCode = QrCode.Create(userContext.UserId, null, qrCodeData, expiresAtUtc);
            await qrCodeRepo.AddAsync(qrCode);

            return Result.Success(new GeneratePointsRedemptionQrResponse
            {
                QrId = qrCode.QrId,
                ExpiresAtUtc = qrCode.ExpiresAt
            });
        }
    }
}
