using System.Security.Cryptography;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.Common;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Application.Users.Command;

public static class ResetPassword
{
    public sealed record ResetPasswordCommand(string Email, string ResetToken, string NewPassword) : ICommand;

    public sealed class Handler(
        IRepository<User> userRepo,
        IRepository<PasswordResetRequest> passwordResetRequestRepo,
        IPasswordHasher passwordHasher) : ICommandHandler<ResetPasswordCommand>
    {
        public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var email = Email.Create(command.Email);

            User? user = await userRepo.Find(new UserByEmailSpecification(email))
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure(UserErrors.NotFoundByEmail);
            }

            PasswordResetRequest? request = await passwordResetRequestRepo
                .Find(new PasswordResetRequestByUserIdSpecification(user.UserId))
                .SingleOrDefaultAsync(cancellationToken);

            if (request is null)
            {
                return Result.Failure(UserErrors.InvalidPasswordResetToken);
            }

            if (request.IsExpired())
            {
                await passwordResetRequestRepo.DeleteAsync(request);
                return Result.Failure(UserErrors.PasswordResetTokenExpired);
            }

            string resetTokenHash = HashResetToken(command.ResetToken);
            if (!SecureCompare(request.TokenHash, resetTokenHash))
            {
                return Result.Failure(UserErrors.InvalidPasswordResetToken);
            }

            string newPasswordHash = passwordHasher.Hash(command.NewPassword);
            user.ChangePasswordHash(newPasswordHash);
            await passwordResetRequestRepo.DeleteAsync(request);

            return Result.Success();
        }

        private static string HashResetToken(string resetToken)
        {
            byte[] hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(resetToken));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool SecureCompare(string hash1, string hash2)
        {
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(hash1),
                Encoding.UTF8.GetBytes(hash2));
        }
    }
}


