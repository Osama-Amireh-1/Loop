using System;
using System.Collections.Generic;
using System.Text;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Shops.Contract;
using Loop.Domain.Shops;
using Loop.Domain.Shops.Specificarions;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Shops.Query;

public sealed class GetShops
{

    public sealed record Query(Guid mallId, Guid? categoryId, string? searchTerm) : IQuery<List<GetShopsResponse>>;


    public sealed class Handler(IReadOnlyRepository<Shop> _shopReadRepo)
        : IQueryHandler<Query, List<GetShopsResponse>>
    {
        public async Task<Result<List<GetShopsResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            string? normalizedSearch = request.searchTerm?.Trim();

            var shops = await _shopReadRepo.Find(new ShopByMallSpecification(request.mallId))
                .Where(s => s.IsActive&& (request.categoryId == null || s.CategoryId == request.categoryId) &&
                            (string.IsNullOrEmpty(normalizedSearch) ||
                             EF.Functions.ILike(s.Name, $"%{normalizedSearch}%")))
                .Select(s => new GetShopsResponse
                {
                    ShopId = s.ShopId,
                    ShopName = s.Name,
                    ShopImageUrl = s.CoverImageUrl,
                    CategoryName = s.Category.Name
                })
                .ToListAsync(cancellationToken);

            return Result.Success(shops);
        }
    }
}
