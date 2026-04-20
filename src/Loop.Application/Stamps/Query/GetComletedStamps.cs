using System;
using System.Collections.Generic;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Stamps.Contract;
using Loop.Domain.Stamps;
using Loop.Domain.Stamps.Specificarions;
using Loop.Domain.Stamps.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Stamps.Query;

public sealed class GetComletedStamps
{
    public sealed record Query() : IQuery<List<GetComletedStampsResponse>>;

    public sealed class Handler(IReadOnlyRepository<UserStampCard> _userStampCardReadRepo, IReadOnlyRepository<StampRedemption> _stampRedemptioReadRepo, IUserContext userContext) : IQueryHandler<Query, List<GetComletedStampsResponse>>
    {
        public async Task<Result<List<GetComletedStampsResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {

            var completedStamps = await (
                from usc in _userStampCardReadRepo.Find(new ActiveUserStampCardsWithDetailsSpecification(userContext.UserId))
                                                  .Where(usc => usc.IsCompleted)
                join sr in _stampRedemptioReadRepo.Find(new StampRedemptionByUserSpecification(userContext.UserId))
                    on new { usc.UserId, usc.Stamp.ShopId, usc.StampId }
                    equals new { sr.UserId, sr.ShopId, sr.StampId }
                    into redemptions
                from sr in redemptions.DefaultIfEmpty()
                where sr == null
                select new GetComletedStampsResponse
                {
                    StampId = usc.StampId,
                    ShopName = usc.Stamp.Shop.Name,
                    StampDescription = usc.Stamp.Description,
                    IconUrl = usc.Stamp.StampIconUrl
                }
            ).ToListAsync(cancellationToken);

            return Result.Success(completedStamps);
        }
    }
}
