using System.Security.Cryptography;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Users.Contract;
using Loop.Domain.Common;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Application.Users.Command;

public static class LoginUser
{
    public sealed record LoginUserCommand(string Email, string Password) : ICommand<AuthTokensResponse>;

    public sealed class Handler(
        IRepository<User> userRepo,
        IRepository<UserSession> userSessionRepo,
        IPasswordHasher passwordHasher,
        ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, AuthTokensResponse>
    {
        public async Task<Result<AuthTokensResponse>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
        {
            var email = Email.Create(command.Email);

            User? user = await userRepo.Find(new UserByEmailSpecification(email))
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<AuthTokensResponse>(UserErrors.NotFoundByEmail);
            }

            bool verified = passwordHasher.Verify(command.Password, user.PasswordHash);

            if (!verified)
            {
                return Result.Failure<AuthTokensResponse>(UserErrors.NotFoundByEmail);
            }

            string accessToken = tokenProvider.CreateAccessToken(user);
            (string refreshToken, DateTime refreshTokenExpiresAtUtc) = tokenProvider.CreateRefreshToken();
            string refreshTokenHash = HashRefreshToken(refreshToken);

            var session = UserSession.Create(user.UserId, refreshTokenHash, refreshTokenExpiresAtUtc);
            await userSessionRepo.AddAsync(session);

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


