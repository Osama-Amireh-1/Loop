using Loop.Application.Abstractions.Authentication;
using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Stamps.Contract;
using Loop.Domain.Stamps;
using Loop.Domain.Stamps.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Loop.Application.Stamps.Query;

public sealed class GetActiveShopStamps
{
    public sealed record Query : IQuery<List<GetActiveShopStampsResponse>>;

    public sealed class Handler(
        IReadOnlyRepository<Stamp> stampReadRepo,
        IShopAdminContext shopAdminContext)
        : IQueryHandler<Query, List<GetActiveShopStampsResponse>>
    {
        public async Task<Result<List<GetActiveShopStampsResponse>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var stamps = await stampReadRepo
                .Find(new ActiveStampsByShopSpecification(shopAdminContext.ShopId))
                .Select(s => new GetActiveShopStampsResponse
                {
                    StampId = s.StampId,
                    StampName = s.Name,
                    StampDescription = s.Description,
                    ImageUrl = s.ImageUrl,
                    IconUrl = s.StampIconUrl,
                    StampsRequired = s.StampsRequired,
                    RewardType = s.RewardType.ToString(),
                    StartDate = s.StartDate,
                    EndDate = s.EndDate
                })
                .ToListAsync(cancellationToken);

            return Result.Success(stamps);
        }
    }
}
