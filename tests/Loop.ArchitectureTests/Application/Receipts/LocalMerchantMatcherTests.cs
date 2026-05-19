using System.Linq.Expressions;
using Loop.Application.Interfaces;
using Loop.Application.Receipts.Contract;
using Loop.Infrastructure.Receipts.MerchantMatcher;
using Loop.Domain.Shops;
using Loop.Domain.Shops.Specifications;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Receipts;

public class LocalMerchantMatcherTests
{
    [Fact]
    public async Task MatchAsync_ShouldMarkReceiptAsPending_WhenMatchScoreIsBelowMinimumThreshold()
    {
        var mallId = Guid.NewGuid();
        var shops = new[]
        {
            Shop.Create(mallId, "Star Market", Guid.NewGuid())
        };

        var repository = new ShopReadOnlyRepositoryStub(shops);
        var matcher = new LocalMerchantMatcher(repository);

        var result = await matcher.MatchAsync(
            mallId,
            new ReceiptOcrResult
            {
                StoreName = "ZZZ",
                MerchantName = "ZZZ",
                Subtotal = 12.5m,
                Currency = "JOD"
            },
            CancellationToken.None);

        result.IsPendingReview.ShouldBeTrue();
        result.MatchedShopId.ShouldBe(shops[0].ShopId);
        result.MatchedShopName.ShouldBe(shops[0].Name);
        result.MatchScore.ShouldNotBeNull();
        result.MatchScore!.Value.ShouldBeLessThan(0.35);
    }

    [Fact]
    public async Task MatchAsync_ShouldNotMarkReceiptAsPending_WhenMatchScoreIsAboveMinimumThreshold()
    {
        var mallId = Guid.NewGuid();
        var shops = new[]
        {
            Shop.Create(mallId, "Carrefour", Guid.NewGuid())
        };

        var repository = new ShopReadOnlyRepositoryStub(shops);
        var matcher = new LocalMerchantMatcher(repository);

        var result = await matcher.MatchAsync(
            mallId,
            new ReceiptOcrResult
            {
                StoreName = "Carrefour",
                MerchantName = "Carrefour",
                Subtotal = 20m,
                Currency = "JOD"
            },
            CancellationToken.None);

        result.IsPendingReview.ShouldBeFalse();
        result.MatchedShopId.ShouldBe(shops[0].ShopId);
        result.MatchedShopName.ShouldBe(shops[0].Name);
        result.MatchScore.ShouldNotBeNull();
        result.MatchScore!.Value.ShouldBe(1);
    }

    [Fact]
    public async Task MatchAsync_ShouldReturnNormalizedReceipt_WhenMerchantIdentityIsMissing()
    {
        var repository = new ShopReadOnlyRepositoryStub([]);
        var matcher = new LocalMerchantMatcher(repository);

        var result = await matcher.MatchAsync(
            Guid.NewGuid(),
            new ReceiptOcrResult
            {
                StoreName = "   ",
                MerchantName = null,
                Currency = " JOD "
            },
            CancellationToken.None);

        result.StoreName.ShouldBe(string.Empty);
        result.MerchantName.ShouldBe(string.Empty);
        result.Currency.ShouldBe("JOD");
        result.MatchedShopId.ShouldBeNull();
        result.MatchScore.ShouldBeNull();
        result.IsPendingReview.ShouldBeFalse();
    }

    [Fact]
    public async Task MatchAsync_ShouldReturnReceiptWithoutMatch_WhenNoShopsExist()
    {
        var repository = new ShopReadOnlyRepositoryStub([]);
        var matcher = new LocalMerchantMatcher(repository);

        var result = await matcher.MatchAsync(
            Guid.NewGuid(),
            new ReceiptOcrResult
            {
                StoreName = "Carrefour",
                MerchantName = "Carrefour",
                Currency = "JOD"
            },
            CancellationToken.None);

        result.StoreName.ShouldBe("Carrefour");
        result.MerchantName.ShouldBe("Carrefour");
        result.MatchedShopId.ShouldBeNull();
        result.MatchedShopName.ShouldBeNull();
        result.MatchScore.ShouldBeNull();
        result.IsPendingReview.ShouldBeFalse();
    }

    [Fact]
    public async Task MatchAsync_ShouldMatchNamesWithDiacriticsNormalization()
    {
        var mallId = Guid.NewGuid();
        var shops = new[]
        {
            Shop.Create(mallId, "CAFÉ MALL", Guid.NewGuid())
        };

        var repository = new ShopReadOnlyRepositoryStub(shops);
        var matcher = new LocalMerchantMatcher(repository);

        var result = await matcher.MatchAsync(
            mallId,
            new ReceiptOcrResult
            {
                StoreName = "Cafe Mall",
                MerchantName = "Cafe Mall"
            },
            CancellationToken.None);

        result.MatchedShopId.ShouldBe(shops[0].ShopId);
        result.MatchedShopName.ShouldBe(shops[0].Name);
        result.IsPendingReview.ShouldBeFalse();
        result.MatchScore.ShouldNotBeNull();
        result.MatchScore!.Value.ShouldBe(1);
    }

    private sealed class ShopReadOnlyRepositoryStub(IEnumerable<Shop> shops) : IReadOnlyRepository<Shop>
    {
        private readonly List<Shop> _shops = shops.ToList();

        public IQueryable<Shop> Find(ISpecification<Shop> spec)
        {
            IQueryable<Shop> query = _shops.AsQueryable();

            if (spec.Criteria is not null)
            {
                query = query.Where(spec.Criteria.Compile()).AsQueryable();
            }

            return new TestAsyncEnumerable<Shop>(query);
        }

        public Task<int> CountAsync(ISpecification<Shop> spec)
        {
            int count = spec.Criteria is null
                ? _shops.Count
                : _shops.Count(spec.Criteria.Compile());

            return Task.FromResult(count);
        }

        public IQueryable<Shop> GetAll() => new TestAsyncEnumerable<Shop>(_shops);

        public int Count(ISpecification<Shop> spec) =>
            spec.Criteria is null
                ? _shops.Count
                : _shops.Count(spec.Criteria.Compile());
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
