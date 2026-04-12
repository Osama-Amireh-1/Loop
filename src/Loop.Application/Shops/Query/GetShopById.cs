using System;
using System.Collections.Generic;
using System.Text;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Shops.Contract;
using Loop.Domain.Offers;
using Loop.Domain.Shops;
using Loop.Domain.Shops.Specificarions;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Shops.Query;

public sealed class GetShopById
{
    public sealed record Query(Guid mallId, Guid shopId) : IQuery<GetShopByIdResponse>;



    public sealed class Handler(IReadOnlyRepository<Shop> _shopReadRepo) : IQueryHandler<Query, GetShopByIdResponse>
    {
        public  async Task<Result<GetShopByIdResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var shop = await _shopReadRepo.Find(new ShopByPKSpecification(request.mallId, request.shopId))
                .Where(s => s.IsActive)
                .Select(s => new GetShopByIdResponse
                {
                    ShopName = s.Name,
                    CategoryName = s.Category.Name,
                    CoverUrl = s.CoverImageUrl,
                    WebsiteLink = s.WebsiteUrl,
                    LogoUrl = s.LogoUrl,
                    SocialLinks = new List<string>()
                }).
                FirstOrDefaultAsync(cancellationToken);

            if(shop is null)
            {
                return Result.Failure<GetShopByIdResponse>(ShopErrors.NotFound(request.shopId));
            }

           
            return Result.Success(shop);
        }
    }

}
