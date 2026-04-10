using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Application.Users.Command;

public static class LogoutUser
{
    public sealed record LogoutUserCommand() : ICommand;

    public sealed class Handler(
        IRepository<User> userRepo,
        IRepository<UserSession> userSessionRepo,
        IUserContext userContext) : ICommandHandler<LogoutUserCommand>
    {
        public async Task<Result> Handle(LogoutUserCommand command, CancellationToken cancellationToken)
        {
            User? user = await userRepo.Find(new UserByIdSpecification(userContext.UserId))
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure(UserErrors.NotFound(userContext.UserId));
            }

            var sessions = await userSessionRepo.Find(new UserSessionsByUserIdSpecification(user.UserId))
                .ToListAsync(cancellationToken);

            await userSessionRepo.DeleteRangeAsync(sessions);

            return Result.Success();
        }
    }
}


