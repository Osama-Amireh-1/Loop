using System;
using System.Collections.Generic;
using System.Text;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Domain.Malls;
using Loop.Domain.Shops;
using Loop.Application.Categories.Contract;
using Loop.Application.Offers.Contract;
using Loop.Domain.Shops.Specifications;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Application.Categories.Query;

public sealed class GetCategories
{
    public sealed record Query(Guid mallId) : IQuery<List<GetCategoriesResponse>>;


    public sealed class Handler(IReadOnlyRepository<Category> _categoryReadRepo)
        : IQueryHandler<Query, List<GetCategoriesResponse>>

    {
        public async Task<Result<List<GetCategoriesResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var categories = await _categoryReadRepo.Find(new CategoryByMallSpecification(request.mallId))
                .Select(c => new GetCategoriesResponse
                {
                    CategoryId=c.CategoryId,
                    CategoryName = c.Name,
                })
                .ToListAsync(cancellationToken);
            return Result.Success(categories);
        }
    }
}


