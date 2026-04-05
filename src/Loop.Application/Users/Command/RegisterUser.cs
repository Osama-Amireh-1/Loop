using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Interfaces;
using Domain.Common;
using Domain.Tiers;
using Domain.Users;
using Domain.Users.Specifications;
using Loop.Domain.Tiers.Specifications;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.Users.Command;

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
            bool emailExists = await userRepo.Find(new UserByEmailSpecification(command.Email)).AnyAsync(cancellationToken);

            if (emailExists)
            {
                return Result.Failure<Guid>(UserErrors.EmailNotUnique);
            }

            string normalizedPhone = command.Phone.Trim();
            bool phoneExists = await userRepo.GetAll().AnyAsync(u => u.Phone.Value == normalizedPhone, cancellationToken);

            if (phoneExists)
            {
                return Result.Failure<Guid>(UserErrors.PhoneNotUnique);
            }

            if (!Enum.TryParse<Gender>(command.Gender, true, out var gender))
            {
                return Result.Failure<Guid>(UserErrors.InvalidGender);
            }
            var tier = await tireReadRepo.Find(new TierByOrderSpecification(1)).FirstOrDefaultAsync(cancellationToken);

            if(tier is null)
            {
                return Result.Failure<Guid>(TierErrors.NotFound(1));
            }

            var user = User.Create(
                firstName: command.FirstName,
                lastName: command.LastName,
                phone: Phone.Create(command.Phone),
                email: Email.Create(command.Email),
                passwordHash: passwordHasher.Hash(command.Password),
                gender: gender,
                defaultTierId: tier.TierId);

            await userRepo.AddAsync(user);
            return user.UserId;
        }
    }
}
