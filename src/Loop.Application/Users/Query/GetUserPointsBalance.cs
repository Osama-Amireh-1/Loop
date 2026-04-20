using System;
using System.Collections.Generic;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Users.Contract;
using Loop.Domain.Configuration;
using Loop.Domain.Configuration.Specifications;
using Loop.Domain.Users;
using Loop.Domain.Users.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Users.Query;

public sealed class GetUserPointsBalance
{
    public sealed record Query(Guid MallId) : IQuery<PointsBalancResponse>;


    public class Handler(
        IReadOnlyRepository<User> userReadRepo,
        IReadOnlyRepository<SystemConfig> systemConfigRepo,
        IUserContext userContext) : IQueryHandler<Query, PointsBalancResponse>
    {
        public async Task<Result<PointsBalancResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var user = await userReadRepo.Find(new UserByPKSpecification(userContext.UserId))
                .Select(u => u.PointsBalance.TotalPoints).FirstOrDefaultAsync(cancellationToken);
            
            if (user == 0)
            {
                return Result.Failure<PointsBalancResponse>(UserErrors.NotFound(userContext.UserId));
            }

            var systemConfig = await systemConfigRepo.Find(new SystemConfigByMallSpecification(request.MallId))
                .FirstOrDefaultAsync(cancellationToken);

            if (systemConfig == null)
            {
                return Result.Failure<PointsBalancResponse>(
                    SystemConfigErrors.NotFound(request.MallId));
            }

            var evaluatedValue = user / systemConfig.PointsToCurrencyRatio;

            return Result.Success(new PointsBalancResponse(user, evaluatedValue));
        }
    }
}
