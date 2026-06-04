using System.Linq.Expressions;
using System.Reflection;
using Loop.Application.Interfaces;
using Loop.Application.Shops.Query;
using Loop.Domain.Shops;
using Loop.Domain.Shops.Specifications;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Shops;

public class GetShopByIdQueryTests
{
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenShopDoesNotExist()
    {
        var repository = new ShopReadOnlyRepositoryStub([]);
        var handler = new GetShopById.Handler(repository);

        var result = await handler.Handle(new GetShopById.Query(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Shops.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenShopIsNotActive()
    {
        var mallId = Guid.NewGuid();
        var category = Category.Create(mallId, "Coffee", "/icons/coffee.png", "Coffee shops", 1);
        var shop = Shop.Create(mallId, "Inactive Shop", category.CategoryId);
        shop.Deactivate();
        AttachCategory(shop, category);

        var repository = new ShopReadOnlyRepositoryStub([shop]);
        var handler = new GetShopById.Handler(repository);

        var result = await handler.Handle(new GetShopById.Query(mallId, shop.ShopId), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Shops.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnShopResponse_WhenShopExistsAndIsActive()
    {
        var mallId = Guid.NewGuid();
        var category = Category.Create(mallId, "Coffee", "/icons/coffee.png", "Coffee shops", 1);
        var shop = Shop.Create(mallId, "Test Cafe", category.CategoryId);
        shop.UpdateDetails(
            "Test Cafe",
            category.CategoryId,
            "A cozy coffee shop",
            "https://cdn.example.com/logo.png",
            "https://cdn.example.com/cover.png",
            @"{""instagram"": ""@testcafe""}",
            "https://testcafe.com");
        AttachCategory(shop, category);

        var repository = new ShopReadOnlyRepositoryStub([shop]);
        var handler = new GetShopById.Handler(repository);

        var result = await handler.Handle(new GetShopById.Query(mallId, shop.ShopId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShopName.ShouldBe("Test Cafe");
        result.Value.CategoryName.ShouldBe("Coffee");
        result.Value.CoverUrl.ShouldBe("https://cdn.example.com/cover.png");
        result.Value.LogoUrl.ShouldBe("https://cdn.example.com/logo.png");
        result.Value.WebsiteLink.ShouldBe("https://testcafe.com");
        result.Value.SocialLinks.Count.ShouldBe(1);
        result.Value.SocialLinks[0].Name.ShouldBe("Instagram");
        result.Value.SocialLinks[0].Link.ShouldBe("@testcafe");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptySocialLinks_WhenSocialLinksJsonIsNull()
    {
        var mallId = Guid.NewGuid();
        var category = Category.Create(mallId, "Bakery", "/icons/bakery.png", "Bakery shops", 2);
        var shop = Shop.Create(mallId, "Bakery Shop", category.CategoryId);
        shop.UpdateDetails(
            "Bakery Shop",
            category.CategoryId,
            "Fresh bread daily",
            "https://cdn.example.com/logo.png",
            "https://cdn.example.com/cover.png",
            null,
            null);
        AttachCategory(shop, category);

        var repository = new ShopReadOnlyRepositoryStub([shop]);
        var handler = new GetShopById.Handler(repository);

        var result = await handler.Handle(new GetShopById.Query(mallId, shop.ShopId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.SocialLinks.ShouldBeEmpty();
        result.Value.WebsiteLink.ShouldBeNull();
    }

    private static void AttachCategory(Shop shop, Category category)
    {
        typeof(Shop)
            .GetProperty(nameof(Shop.Category), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(shop, category);
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
