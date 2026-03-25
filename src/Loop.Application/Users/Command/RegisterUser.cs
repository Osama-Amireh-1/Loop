using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Interfaces;
using Domain.Users;
using Domain.Users.Specifications;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Command;

public static class RegisterUser
{
    public sealed record RegisterUserCommand(string Email, string FirstName, string LastName, string Password)
        : ICommand<Guid>; 

    public sealed class Handler(IRepository<User> userRepo, IPasswordHasher passwordHasher)
        : ICommandHandler<RegisterUserCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            bool userExist = await userRepo.Find(new UserByEmailSpecification(command.Email)).AnyAsync(cancellationToken);

            if (userExist)
            {
                return Result.Failure<Guid>(UserErrors.EmailNotUnique);
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                PasswordHash = passwordHasher.Hash(command.Password)
            };

            user.Raise(new UserRegisteredDomainEvent(user.Id));

            await userRepo.AddAsync(user);
            return user.Id;
        }
    }
}
