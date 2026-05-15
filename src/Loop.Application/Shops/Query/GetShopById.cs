using System;
using System.Collections.Generic;
using System.Text;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Shops.Contract;
using Loop.Application.Shops.Helpers;
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
        public async Task<Result<GetShopByIdResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var shop = await _shopReadRepo.Find(new ShopByPKSpecification(request.mallId, request.shopId))
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync(cancellationToken);

            if (shop is null)
            {
                return Result.Failure<GetShopByIdResponse>(ShopErrors.NotFound(request.shopId));
            }

            var socialLinks = SocialLinksDeserializer.Deserialize(shop.SocialLinks);

            var response = new GetShopByIdResponse
            {
                ShopName = shop.Name,
                CategoryName = shop.Category.Name,
                CoverUrl = shop.CoverImageUrl,
                WebsiteLink = shop.WebsiteUrl,
                LogoUrl = shop.LogoUrl,
                SocialLinks = socialLinks
            };

            return Result.Success(response);
        }
    }
}
