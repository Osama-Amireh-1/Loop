using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Users.Contract;
using Loop.Domain.Common;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Application.Users.Command;

public static class UpdateUser
{
    public sealed record UpdateUserCommand(string? FirstName, string? LastName, string? Phone, string? ProfileImageUrl) : ICommand<UserResponse>;

    public sealed class Handler(
        IRepository<User> userRepo,
        IUserContext userContext)
        : ICommandHandler<UpdateUserCommand, UserResponse>
    {
        public async Task<Result<UserResponse>> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
        {
            User? user = await userRepo.Find(new UserByPKSpecification(userContext.UserId))
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<UserResponse>(UserErrors.NotFound(userContext.UserId));
            }

            string firstName = command.FirstName ?? user.FirstName;
            string lastName = command.LastName ?? user.LastName;
            string? profileImageUrl = command.ProfileImageUrl ?? user.ProfileImageUrl;

            Phone? phone = null;
            if (!string.IsNullOrWhiteSpace(command.Phone))
            {
                var phoneResult = Phone.Create(command.Phone);
                if (phoneResult.IsFailure)
                {
                    return Result.Failure<UserResponse>(phoneResult.Error);
                }

                phone = phoneResult.Value;

                bool phoneExists = await userRepo.GetAll()
                    .AnyAsync(u => u.UserId != user.UserId && u.Phone == phone, cancellationToken);

                if (phoneExists)
                {
                    return Result.Failure<UserResponse>(UserErrors.PhoneNotUnique);
                }
            }

            user.UpdateProfile(firstName, lastName, profileImageUrl, phone);

            return new UserResponse
            {
                Id = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email.Value,
                Phone = user.Phone.Value,
                Gender = user.Gender.ToString(),
                ProfileImageUrl = user.ProfileImageUrl
            };
        }
    }
}
