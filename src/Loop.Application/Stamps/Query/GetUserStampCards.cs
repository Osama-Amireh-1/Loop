using System;
using System.Collections.Generic;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Users.Contract;
using Loop.Domain.Stamps;
using Loop.Domain.Users;
using Loop.Application.Stamps.Contract;
using Loop.Domain.Stamps.Specifications;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Application.Stamps.Query;

public sealed class GetUserStampCards
{
    public sealed record Query() : IQuery<List<GetUserStampCardsResponse>>;



    public sealed class Handler(IReadOnlyRepository<UserStampCard> _userStampCardReadRepo, IUserContext userContext)
    : IQueryHandler<Query, List<GetUserStampCardsResponse>>
    {
        public async Task<Result<List<GetUserStampCardsResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {

            var userStampCards = await _userStampCardReadRepo.Find(new ActiveUserStampCardsWithDetailsSpecification(userContext.UserId))
                .Where(usc=> !usc.IsCompleted)
                .Select(usc => new GetUserStampCardsResponse
                {
                    stampId = usc.StampId,
                    shopName = usc.Stamp.Shop.Name,
                    stampsCounter = usc.StampsCounter,
                    stampsRequired = usc.Stamp.StampsRequired,
                    stampDescription = usc.Stamp.Description,
                    stampName = usc.Stamp.Name,
                    iconUrl = usc.Stamp.StampIconUrl
                })  
                .ToListAsync(cancellationToken);

            return Result.Success(userStampCards);
        }
    }
}


