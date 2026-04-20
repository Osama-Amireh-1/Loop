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

public sealed class GetStamps
{
    public sealed record Query(Guid mallId, Guid shopId) : IQuery<List<GetStampsResponse>>;



    public class Handler(IReadOnlyRepository<UserStampCard> _userStampCardReadRepo, IReadOnlyRepository<Stamp> _stampReadRepo, IUserContext userContext) : IQueryHandler<Query, List<GetStampsResponse>>
    {
        public async Task<Result<List<GetStampsResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var stamps = await (from s in _stampReadRepo.Find(new ActiveStampByPKSpecification(request.mallId, request.shopId))
                                join usc in _userStampCardReadRepo.Find(new ActiveUserStampCardsWithDetailsSpecification(userContext.UserId))
                                     on s.StampId equals usc.StampId into stampsWithUserCards
                                select new GetStampsResponse
                                {
                                    StampName = s.Name,
                                    StampsCounter = stampsWithUserCards.Select(x => x.StampsCounter).FirstOrDefault(),
                                    IconUrl = s.StampIconUrl,
                                    StampsRequired = s.StampsRequired,
                                    StampDescription = s.Description,
                                    IsComplete = stampsWithUserCards.Any(usc => usc.IsCompleted),
                                }).ToListAsync(cancellationToken);

            return Result<List<GetStampsResponse>>.Success(stamps);

        }
    }
}
