using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Users.Contract;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Users.Command;


public sealed class AddPointsToUser
{
    public sealed record Command(
        Guid UserId,
        int Amount
    ) : ICommand<bool>;

    public sealed class Handler(IRepository<User> userRepository) : ICommandHandler<Command, bool>
    {
        public async Task<Result<bool>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            if (request.Amount <= 0)
            {
                return Result.Failure<bool>(
                    new Error(
                        "AddPoints.InvalidAmount",
                        "Points amount must be positive.",
                        ErrorType.Validation));
            }

            var user = await userRepository.Find(new UserByPKSpecification(request.UserId))
                .SingleOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return Result.Failure<bool>(
                    UserErrors.NotFound(request.UserId));
            }

            var creditResult = user.CreditPoints(request.Amount);
            if (!creditResult.IsSuccess)
            {
                return Result.Failure<bool>(creditResult.Error);
            }



            return Result.Success(true);
        }
    }
}
