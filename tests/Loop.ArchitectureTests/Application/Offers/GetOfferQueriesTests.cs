using System.Linq.Expressions;
using System.Reflection;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Offers.Query;
using Loop.Domain.Offers;
using Loop.Domain.Shops;
using Loop.Domain.Specifications;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Offers;

public class GetOfferQueriesTests
{
    [Fact]
    public async Task GetOfferById_ShouldReturnMappedResponse_WhenOfferExists()
    {
        var mallId = Guid.NewGuid();
        var shop = Shop.Create(mallId, "Fresh Market", Guid.NewGuid());
        shop.UpdateDetails(
            "Fresh Market",
            shop.CategoryId,
            "Neighborhood grocery store",
            "https://cdn.example.com/logo.png",
            "https://cdn.example.com/cover.png",
            null,
            null);

        var offer = Offer.Create(
            shop.ShopId,
            "10% Off",
            "Save on your next purchase",
            RewardType.Discount,
            "{\"percent\":10}",
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddHours(1));
        typeof(Offer)
            .GetProperty(nameof(Offer.ImageUrl), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(offer, "https://cdn.example.com/offer.png");
        AttachShop(offer, shop);

        var repository = new OfferReadOnlyRepositoryStub([offer]);
        var handler = new GetOfferById.Handler(repository);

        var result = await handler.Handle(new GetOfferById.Query(offer.OfferId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.OfferId.ShouldBe(offer.OfferId);
        result.Value.OfferName.ShouldBe(offer.Name);
        result.Value.OfferDescription.ShouldBe(offer.Description);
        result.Value.OfferImageUrl.ShouldBe("https://cdn.example.com/offer.png");
        result.Value.RewardType.ShouldBe(offer.RewardType.ToString());
        result.Value.RewardValue.ShouldBe(10m);
        result.Value.ShopId.ShouldBe(shop.ShopId);
        result.Value.ShopName.ShouldBe(shop.Name);
        result.Value.CoverImageUrl.ShouldBe("https://cdn.example.com/cover.png");
    }

    [Fact]
    public async Task GetOfferById_ShouldReturnNotFound_WhenOfferDoesNotExist()
    {
        var repository = new OfferReadOnlyRepositoryStub([]);
        var handler = new GetOfferById.Handler(repository);

        var result = await handler.Handle(new GetOfferById.Query(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Offers.NotFound");
    }

    [Fact]
    public async Task GetOffersByShop_ShouldExcludeConfirmedRedemptions()
    {
        var mallId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var shop = Shop.Create(mallId, "Cafe Loop", categoryId);
        shop.UpdateDetails(
            "Cafe Loop",
            categoryId,
            "Coffee and desserts",
            "https://cdn.example.com/logo.png",
            "https://cdn.example.com/cover.png",
            null,
            null);

        var availableOffer = Offer.Create(
            shop.ShopId,
            "Free drink",
            "Redeem a free drink",
            RewardType.Discount,
            "{\"percent\":100}",
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddHours(1));
        AttachShop(availableOffer, shop);

        var userId = Guid.NewGuid();
        var redeemedOffer = Offer.Create(
            shop.ShopId,
            "Buy one get one",
            "Second drink is free",
            RewardType.Discount,
            "{\"percent\":50}",
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddHours(1));
        AttachShop(redeemedOffer, shop);
        redeemedOffer.Redeem(userId, shop.ShopId).Confirm();

        var repository = new OfferReadOnlyRepositoryStub([availableOffer, redeemedOffer]);
        IUserContext userContext = new TestUserContext(userId);
        var handler = new GetOffersByShop.Handler(repository, userContext);

        var result = await handler.Handle(new GetOffersByShop.Query(mallId, shop.ShopId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value[0].OfferId.ShouldBe(availableOffer.OfferId);
        result.Value[0].OfferDescription.ShouldBe(availableOffer.Description);
        result.Value[0].CoverImageUrl.ShouldBe("https://cdn.example.com/cover.png");
    }

    private static void AttachShop(Offer offer, Shop shop)
    {
        typeof(Offer)
            .GetProperty(nameof(Offer.Shop), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(offer, shop);
    }

    private sealed class TestUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }

    private sealed class OfferReadOnlyRepositoryStub(IEnumerable<Offer> offers) : IReadOnlyRepository<Offer>
    {
        private readonly List<Offer> _offers = offers.ToList();

        public IQueryable<Offer> Find(ISpecification<Offer> spec)
        {
            IQueryable<Offer> query = _offers.AsQueryable();

            if (spec.Criteria is not null)
            {
                query = query.Where(spec.Criteria.Compile()).AsQueryable();
            }

            return new TestAsyncEnumerable<Offer>(query);
        }

        public Task<int> CountAsync(ISpecification<Offer> spec)
        {
            int count = spec.Criteria is null
                ? _offers.Count
                : _offers.Count(spec.Criteria.Compile());

            return Task.FromResult(count);
        }

        public IQueryable<Offer> GetAll() => new TestAsyncEnumerable<Offer>(_offers);

        public int Count(ISpecification<Offer> spec) =>
            spec.Criteria is null
                ? _offers.Count
                : _offers.Count(spec.Criteria.Compile());
    }

    private sealed class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);

        public object Execute(Expression expression) => inner.Execute(expression)!;

        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            Type expectedResultType = typeof(TResult).GetGenericArguments()[0];
            object? executionResult = typeof(IQueryProvider)
                .GetMethods()
                .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethod)
                .MakeGenericMethod(expectedResultType)
                .Invoke(inner, [expression]);

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, [executionResult])!;
        }
    }

    private sealed class TestAsyncEnumerable<T>(IEnumerable<T> enumerable)
        : EnumerableQuery<T>(enumerable), IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(Expression expression)
            : this(new EnumerableQuery<T>(expression))
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;

        public ValueTask DisposeAsync()
        {
            inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    }
}