using Loop.Application.Abstractions.Messaging;
using Loop.Application.Interfaces;
using Loop.Application.Offers.Contract;
using Loop.Domain.Offers;
using Loop.Domain.Offers.Specifications;
using Loop.SharedKernel;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Loop.Application.Offers.Query;

public sealed class GetOfferById
{
    public sealed record Query(Guid OfferId) : IQuery<GetOfferByIdResponse>;

    public sealed class Handler(
        IReadOnlyRepository<Offer> offerReadRepo)
        : IQueryHandler<Query, GetOfferByIdResponse>
    {
        public async Task<Result<GetOfferByIdResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var offer = await offerReadRepo
                .Find(new OfferByIdWithRedemptionsSpecification(request.OfferId))
                .SingleOrDefaultAsync(cancellationToken);

            if (offer is null)
            {
                return Result.Failure<GetOfferByIdResponse>(OfferErrors.NotFound(request.OfferId));
            }

            decimal rewardValue = 0m;

            if (!string.IsNullOrWhiteSpace(offer.RewardValue))
            {
                try
                {
                    using var document = JsonDocument.Parse(offer.RewardValue);
                    var root = document.RootElement;

                    if (root.TryGetProperty("percent", out var percentElement) && percentElement.TryGetDecimal(out var percent))
                    {
                        rewardValue = percent;
                    }
                    else if (root.TryGetProperty("value", out var valueElement) && valueElement.TryGetDecimal(out var value))
                    {
                        rewardValue = value;
                    }
                }
                catch (JsonException)
                {
                    rewardValue = 0m;
                }
            }

            var response = new GetOfferByIdResponse
            {
                OfferId = offer.OfferId,
                OfferName = offer.Name,
                OfferDescription = offer.Description,
                OfferImageUrl = offer.ImageUrl,
                RewardType = offer.RewardType.ToString(),
                RewardValue = rewardValue,
                ShopId = offer.ShopId,
                ShopName = offer.Shop.Name,
                CoverImageUrl = offer.Shop.CoverImageUrl,
            };

            return Result.Success(response);
        }
    }
}
