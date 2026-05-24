using System;
using System.Collections.Generic;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Categories.Contract;
using Loop.Application.Interfaces;
using Loop.Application.Offers.Contract;
using Loop.Domain.Offers;
using Loop.Domain.Offers.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Offers.Query;

public sealed class GetOffersByShop
{
    public sealed record Query(Guid mallId, Guid shopId) : IQuery<List<GetOffersByShopResponse>>;

    public sealed class Handler(
        IReadOnlyRepository<Offer> offerReadRepo,
        IUserContext userContext)
       : IQueryHandler<Query, List<GetOffersByShopResponse>>
    {
        public async Task<Result<List<GetOffersByShopResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var offers = await offerReadRepo
                .Find(new ActiveOfferByPKSpecification(request.mallId, request.shopId))
                .Where(g => !g.Redemptions.Any(r => r.UserId == userContext.UserId && r.Status == OfferRedemptionStatus.Confirmed))
                .Select(g => new GetOffersByShopResponse
                {
                    OfferId = g.OfferId,
                    CoverImageUrl = g.Shop.CoverImageUrl,
                    OfferDescription = g.Description
                })
                .ToListAsync(cancellationToken);

            return Result.Success(offers);
        }
    }
}
