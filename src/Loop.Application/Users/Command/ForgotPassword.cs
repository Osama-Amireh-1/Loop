using System.Security.Cryptography;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Communication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.Common;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Application.Users.Command;

public static class ForgotPassword
{
    public sealed record ForgotPasswordCommand(string Email) : ICommand<string>;

    public sealed class Handler(
        IReadOnlyRepository<User> userRepo,
        IRepository<PasswordResetRequest> passwordResetRequestRepo,
        IEmailSender emailSender) : ICommandHandler<ForgotPasswordCommand, string>
    {
        public async Task<Result<string>> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            var email = Email.Create(command.Email);

            User? user = await userRepo.Find(new UserByEmailSpecification(email))
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Success("If the email exists in our system, a password reset link will be sent shortly.");
            }

            string resetToken = GenerateResetToken();
            string resetTokenHash = HashResetToken(resetToken);
            DateTime expiresAtUtc = DateTime.UtcNow.AddHours(1);

            PasswordResetRequest? existingRequest = await passwordResetRequestRepo
                .Find(new PasswordResetRequestByUserIdSpecification(user.UserId))
                .SingleOrDefaultAsync(cancellationToken);

            if (existingRequest is not null)
            {
              await  passwordResetRequestRepo.DeleteAsync(existingRequest);
            }

            var request = PasswordResetRequest.Create(user.UserId, resetTokenHash, expiresAtUtc);
            await passwordResetRequestRepo.AddAsync(request);

            string subject = "Password reset request";
            string body = $"Use this password reset token to complete your request: {resetToken}\n\nThis token expires at {expiresAtUtc:u}.";

            await emailSender.SendAsync(user.Email.Value, subject, body, cancellationToken);

            return Result.Success("If the email exists in our system, a password reset link will be sent shortly.");
        }

        private static string GenerateResetToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }

        private static string HashResetToken(string resetToken)
        {
            byte[] hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(resetToken));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}


