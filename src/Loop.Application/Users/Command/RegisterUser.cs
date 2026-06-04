using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.Common;
using Loop.Domain.Tiers;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Loop.Domain.Tiers.Specifications;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Application.Users.Command;

public static class RegisterUser
{
    public sealed record RegisterUserCommand(
        string Email,
        string FirstName,
        string LastName,
        string Phone,
        string Gender,
        string Password)
        : ICommand<Guid>;

    public sealed class Handler(IRepository<User> userRepo,IReadOnlyRepository<Tier> tireReadRepo ,IPasswordHasher passwordHasher)
        : ICommandHandler<RegisterUserCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            var emailResult = Email.Create(command.Email);
            if (emailResult.IsFailure)
            {
                return Result.Failure<Guid>(emailResult.Error);
            }

            bool emailExists = await userRepo.Find(new UserByEmailSpecification(emailResult.Value)).AnyAsync(cancellationToken);

            if (emailExists)
            {
                return Result.Failure<Guid>(UserErrors.EmailNotUnique);
            }

            var phoneResult = Phone.Create(command.Phone);
            if (phoneResult.IsFailure)
            {
                return Result.Failure<Guid>(phoneResult.Error);
            }

            bool phoneExists = await userRepo.GetAll()
    .AnyAsync(u => u.Phone.Value == phoneResult.Value.Value, cancellationToken);

            if (phoneExists)
            {
                return Result.Failure<Guid>(UserErrors.PhoneNotUnique);
            }

            if (!Enum.TryParse<Gender>(command.Gender, true, out var gender))
            {
                return Result.Failure<Guid>(UserErrors.InvalidGender);
            }

            var tier = await tireReadRepo.Find(new TierByOrderSpecification(1)).FirstOrDefaultAsync(cancellationToken);

            if (tier is null)
            {
                return Result.Failure<Guid>(TierErrors.NotFound(1));
            }

            var user = User.Create(
                firstName: command.FirstName,
                lastName: command.LastName,
                phone: phoneResult.Value,
                email: emailResult.Value,
                passwordHash: passwordHasher.Hash(command.Password),
                gender: gender,
                defaultTierId: tier.TierId);

            await userRepo.AddAsync(user);
            return user.UserId;
        }
    }
}




