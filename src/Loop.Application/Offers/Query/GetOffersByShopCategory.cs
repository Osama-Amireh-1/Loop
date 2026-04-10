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


    public sealed class Handler(IReadOnlyRepository<Offer> _offerReadRepo, IUserContext userContext)
: IQueryHandler<Query, List<GetOffersByShopCategoryResponse>>
    {
        public async Task<Result<List<GetOffersByShopCategoryResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var offers = await _offerReadRepo
                    .Find(new OfferByMallSpecification(request.mallId))
                    .Where(o => !o.Redemptions.Any(r => r.UserId == userContext.UserId))
                    .GroupBy(o => new { o.Shop.CategoryId, o.Shop.Category.Name })
                    .Select(g => new GetOffersByShopCategoryResponse
                    {
                        CategoryName = g.Key.Name,
                        Offers = g.Select(o => new OfferItem
                        {
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


