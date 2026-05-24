using System;
using System.Collections.Generic;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.Offers;
using Loop.Domain.Stamps;
using Loop.Application.Offers.Contract;
using Loop.Application.Stamps.Contract;
using Loop.Domain.Offers.Specifications;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Application.Offers.Query;

public sealed class GetOffersByShopCategory
{
    public sealed record Query(Guid mallId) : IQuery<List<GetOffersByShopCategoryResponse>>;

    public sealed class Handler(
        IReadOnlyRepository<Offer> offerReadRepo,
        IUserContext userContext)
        : IQueryHandler<Query, List<GetOffersByShopCategoryResponse>>
    {
        public async Task<Result<List<GetOffersByShopCategoryResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var offers = await offerReadRepo
                .Find(new ActiveOfferByMallSpecification(request.mallId))
                .Where(o => !o.Redemptions.Any(r => r.UserId == userContext.UserId && r.Status == OfferRedemptionStatus.Confirmed))
                .GroupBy(o => new { o.Shop.CategoryId })
                .Select(g => new GetOffersByShopCategoryResponse
                {
                    CategoryName = g.Select(o => o.Shop.Category.Name).First(),
                    Offers = g.Select(o => new OfferItem
                    {
                        OfferId = o.OfferId,
                        ShopName = o.Shop.Name,
                        OfferDescription = o.Description,
                        CoverImageUrl = o.Shop.CoverImageUrl,
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            return Result.Success(offers);
        }
    }
}


