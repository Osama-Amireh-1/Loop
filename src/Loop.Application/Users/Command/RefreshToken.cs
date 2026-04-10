using System.Security.Cryptography;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Users.Contract;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Application.Users.Command;

public static class RefreshToken
{
    public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthTokensResponse>;

    public sealed class Handler(
        IRepository<UserSession> userSessionRepo,
        IReadOnlyRepository<User> userRepo,
        ITokenProvider tokenProvider) : ICommandHandler<RefreshTokenCommand, AuthTokensResponse>
    {
        public async Task<Result<AuthTokensResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            string refreshTokenHash = HashRefreshToken(command.RefreshToken);

            UserSession? session = await userSessionRepo.Find(new UserSessionByRefreshTokenHashSpecification(refreshTokenHash))
                .SingleOrDefaultAsync(cancellationToken);

            if (session is null)
            {
                return Result.Failure<AuthTokensResponse>(UserErrors.InvalidRefreshToken);
            }

            if (session.IsExpired())
            {
               await userSessionRepo.DeleteAsync(session);
                return Result.Failure<AuthTokensResponse>(UserErrors.RefreshTokenExpired);
            }

            User? user = await userRepo.Find(new UserByIdSpecification(session.UserId))
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
            {
              await  userSessionRepo.DeleteAsync(session);
                return Result.Failure<AuthTokensResponse>(UserErrors.InvalidRefreshToken);
            }

            string accessToken = tokenProvider.CreateAccessToken(user);
            (string newRefreshToken, DateTime refreshTokenExpiresAtUtc) = tokenProvider.CreateRefreshToken();
            string newRefreshTokenHash = HashRefreshToken(newRefreshToken);

           await userSessionRepo.DeleteAsync(session);
            var newSession = UserSession.Create(user.UserId, newRefreshTokenHash, refreshTokenExpiresAtUtc);
            await userSessionRepo.AddAsync(newSession);

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


