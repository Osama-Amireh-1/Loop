using Application.Abstractions.Messaging;
using Application.Abstractions.Authentication;
using Application.Interfaces;
using Domain.Users;
using Domain.Users.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Command;

public static class LoginUser
{
    public sealed record LoginUserCommand(string Email, string Password) : ICommand<string>;

    public sealed class Handler(
        IReadOnlyRepository<User> userReadOnlyRepo,
        IPasswordHasher passwordHasher,
        ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, string>  
    {
        public async Task<Result<string>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
        {
            User? user = await userReadOnlyRepo.Find(new UserByEmailSpecification(command.Email))
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<string>(UserErrors.NotFoundByEmail);
            }

            bool verified = passwordHasher.Verify(command.Password, user.PasswordHash);

            if (!verified)
            {
                return Result.Failure<string>(UserErrors.NotFoundByEmail);
            }

            string token = tokenProvider.Create(user);

            return token;
        }
    }
}
