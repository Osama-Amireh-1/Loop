using System.Security.Cryptography;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Users.Contract;
using Loop.Domain.Shops;
using Loop.Domain.Shops.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Shops.Command;

public static class RefreshShopAdminToken
{
    public sealed record RefreshShopAdminTokenCommand(string RefreshToken) : ICommand<AuthTokensResponse>;

    public sealed class Handler(
        IRepository<ShopAdminSession> shopAdminSessionRepo,
        IReadOnlyRepository<ShopAdmin> shopAdminRepo,
        ITokenProvider tokenProvider) : ICommandHandler<RefreshShopAdminTokenCommand, AuthTokensResponse>
    {
        public async Task<Result<AuthTokensResponse>> Handle(RefreshShopAdminTokenCommand command, CancellationToken cancellationToken)
        {
            string refreshTokenHash = HashRefreshToken(command.RefreshToken);

            ShopAdminSession? session = await shopAdminSessionRepo.Find(new ShopAdminSessionByRefreshTokenHashSpecification(refreshTokenHash))
                .SingleOrDefaultAsync(cancellationToken);

            if (session is null)
            {
                return Result.Failure<AuthTokensResponse>(ShopErrors.InvalidRefreshToken);
            }

            if (session.IsExpired())
            {
                await shopAdminSessionRepo.DeleteAsync(session);
                return Result.Failure<AuthTokensResponse>(ShopErrors.RefreshTokenExpired);
            }

            ShopAdmin? shopAdmin = await shopAdminRepo.Find(new ShopAdminByPKSpecification(session.ShopAdminId))
                .SingleOrDefaultAsync(cancellationToken);

            if (shopAdmin is null)
            {
                await shopAdminSessionRepo.DeleteAsync(session);
                return Result.Failure<AuthTokensResponse>(ShopErrors.InvalidRefreshToken);
            }

            string accessToken = tokenProvider.CreateAccessToken(shopAdmin);
            (string newRefreshToken, DateTime refreshTokenExpiresAtUtc) = tokenProvider.CreateRefreshToken();
            string newRefreshTokenHash = HashRefreshToken(newRefreshToken);

            await shopAdminSessionRepo.DeleteAsync(session);
            var newSession = ShopAdminSession.Create(shopAdmin.ShopAdminId, newRefreshTokenHash, refreshTokenExpiresAtUtc);
            await shopAdminSessionRepo.AddAsync(newSession);

            return new AuthTokensResponse(accessToken, newRefreshToken);
        }

        private static string HashRefreshToken(string refreshToken)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(refreshToken);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
