using System;
using System.Collections.Generic;
using System.Text;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Shops.Contract;
using Loop.Domain.Shops;
using Loop.Domain.Shops.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Shops.Query;

public sealed class GetShops
{
    public sealed record Query(
        Guid mallId,
        Guid? categoryId,
        string? searchTerm,
        int pageNumber = 1,
        int pageSize = 10) : IQuery<PaginatedShopsResponse>;

    public sealed class Handler(IReadOnlyRepository<Shop> shopReadRepo)
        : IQueryHandler<Query, PaginatedShopsResponse>
    {
        public async Task<Result<PaginatedShopsResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Validate pagination parameters
            if (request.pageNumber < 1)
            {
                return Result.Failure<PaginatedShopsResponse>(
                    new Error("Pagination.InvalidPageNumber", "Page number must be greater than 0", ErrorType.Validation));
            }

            if (request.pageSize < 1 || request.pageSize > 100)
            {
                return Result.Failure<PaginatedShopsResponse>(
                    new Error("Pagination.InvalidPageSize", "Page size must be between 1 and 100", ErrorType.Validation));
            }

            string? normalizedSearch = request.searchTerm?.Trim();

            var baseQuery = shopReadRepo.Find(new ShopByMallSpecification(request.mallId))
                .Where(s => s.IsActive &&
                            (request.categoryId == null || s.CategoryId == request.categoryId) &&
                            (string.IsNullOrEmpty(normalizedSearch) ||
                             EF.Functions.ILike(s.Name, $"%{normalizedSearch}%")));

            int totalCount = await baseQuery.CountAsync(cancellationToken);

            int skip = (request.pageNumber - 1) * request.pageSize;
            int totalPages = (int)Math.Ceiling((double)totalCount / request.pageSize);

            var shops = await baseQuery
                .Skip(skip)
                .Take(request.pageSize)
                .Select(s => new GetShopsResponse
                {
                    ShopId = s.ShopId,
                    ShopName = s.Name,
                    ShopImageUrl = s.LogoUrl,
                    CategoryName = s.Category.Name
                })
                .OrderBy(s=>s.ShopName)
                .ToListAsync(cancellationToken);

            var response = new PaginatedShopsResponse
            {
                Data = shops,
                PageNumber = request.pageNumber,
                PageSize = request.pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            return Result.Success(response);
        }
    }
}
