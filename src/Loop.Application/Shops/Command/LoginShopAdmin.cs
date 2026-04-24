using System.Security.Cryptography;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Users.Contract;
using Loop.Domain.Common;
using Loop.Domain.Shops;
using Loop.Domain.Shops.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Shops.Command;

public static class LoginShopAdmin
{
    public sealed record LoginShopAdminCommand(string Email, string Password) : ICommand<AuthTokensResponse>;

    public sealed class Handler(
        IRepository<ShopAdmin> shopAdminRepo,
        IRepository<ShopAdminSession> shopAdminSessionRepo,
        IPasswordHasher passwordHasher,
        ITokenProvider tokenProvider) : ICommandHandler<LoginShopAdminCommand, AuthTokensResponse>
    {
        public async Task<Result<AuthTokensResponse>> Handle(LoginShopAdminCommand command, CancellationToken cancellationToken)
        {
            var email = Email.Create(command.Email);

            ShopAdmin? shopAdmin = await shopAdminRepo.Find(new ShopAdminByEmailSpecification(email))
                .SingleOrDefaultAsync(cancellationToken);

            if (shopAdmin is null)
            {
                return Result.Failure<AuthTokensResponse>(ShopErrors.AdminNotFoundByEmail);
            }

            bool verified = passwordHasher.Verify(command.Password, shopAdmin.PasswordHash);

            if (!verified)
            {
                return Result.Failure<AuthTokensResponse>(ShopErrors.AdminNotFoundByEmail);
            }

            string accessToken = tokenProvider.CreateAccessToken(shopAdmin);
            (string refreshToken, DateTime refreshTokenExpiresAtUtc) = tokenProvider.CreateRefreshToken();
            string refreshTokenHash = HashRefreshToken(refreshToken);

            var session = ShopAdminSession.Create(shopAdmin.ShopAdminId, refreshTokenHash, refreshTokenExpiresAtUtc);
            await shopAdminSessionRepo.AddAsync(session);

            return new AuthTokensResponse(accessToken, refreshToken);
        }

        private static string HashRefreshToken(string refreshToken)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(refreshToken);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
