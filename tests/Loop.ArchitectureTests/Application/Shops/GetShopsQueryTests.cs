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

public class GetShopsQueryTests
{
    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenPageNumberIsLessThanOne()
    {
        var stub = new ShopReadOnlyRepositoryStub([]);
        var handler = new GetShops.Handler(stub);

        var result = await handler.Handle(
            new GetShops.Query(Guid.NewGuid(), null, null, pageNumber: 0, pageSize: 10),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Pagination.InvalidPageNumber");
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenPageSizeIsLessThanOne()
    {
        var stub = new ShopReadOnlyRepositoryStub([]);
        var handler = new GetShops.Handler(stub);

        var result = await handler.Handle(
            new GetShops.Query(Guid.NewGuid(), null, null, pageNumber: 1, pageSize: 0),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Pagination.InvalidPageSize");
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenPageSizeIsGreaterThanOneHundred()
    {
        var stub = new ShopReadOnlyRepositoryStub([]);
        var handler = new GetShops.Handler(stub);

        var result = await handler.Handle(
            new GetShops.Query(Guid.NewGuid(), null, null, pageNumber: 1, pageSize: 101),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Pagination.InvalidPageSize");
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedShops_WhenShopsExist()
    {
        var mallId = Guid.NewGuid();
        var category = Category.Create(mallId, "Coffee", "/icons/coffee.png", "Coffee shops", 1);
        var shops = Enumerable.Range(1, 5).Select(i =>
        {
            var shop = Shop.Create(mallId, $"Shop {i}", category.CategoryId);
            shop.UpdateDetails(
                $"Shop {i}",
                category.CategoryId,
                $"Description {i}",
                $"https://cdn.example.com/logo{i}.png",
                $"https://cdn.example.com/cover{i}.png",
                null,
                null);
            AttachCategory(shop, category);
            return shop;
        }).ToList();

        var stub = new ShopReadOnlyRepositoryStub(shops);
        var handler = new GetShops.Handler(stub);

        var result = await handler.Handle(
            new GetShops.Query(mallId, null, null, pageNumber: 1, pageSize: 3),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.PageNumber.ShouldBe(1);
        result.Value.PageSize.ShouldBe(3);
        result.Value.TotalCount.ShouldBe(5);
        result.Value.TotalPages.ShouldBe(2);
        result.Value.Data.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyData_WhenNoShopsExist()
    {
        var mallId = Guid.NewGuid();
        var stub = new ShopReadOnlyRepositoryStub([]);
        var handler = new GetShops.Handler(stub);

        var result = await handler.Handle(
            new GetShops.Query(mallId, null, null, pageNumber: 1, pageSize: 10),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Data.ShouldBeEmpty();
        result.Value.TotalCount.ShouldBe(0);
        result.Value.TotalPages.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_ShouldFilterByCategoryId()
    {
        var mallId = Guid.NewGuid();
        var coffeeCategory = Category.Create(mallId, "Coffee", "/icons/coffee.png", "Coffee shops", 1);
        var bakeryCategory = Category.Create(mallId, "Bakery", "/icons/bakery.png", "Bakery shops", 2);

        var coffeeShop = Shop.Create(mallId, "Coffee Shop", coffeeCategory.CategoryId);
        AttachCategory(coffeeShop, coffeeCategory);

        var bakeryShop = Shop.Create(mallId, "Bakery Shop", bakeryCategory.CategoryId);
        AttachCategory(bakeryShop, bakeryCategory);

        var stub = new ShopReadOnlyRepositoryStub([coffeeShop, bakeryShop]);
        var handler = new GetShops.Handler(stub);

        var result = await handler.Handle(
            new GetShops.Query(mallId, coffeeCategory.CategoryId, null, pageNumber: 1, pageSize: 10),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Data.Count.ShouldBe(1);
        result.Value.Data[0].ShopName.ShouldBe("Coffee Shop");
    }

    [Fact]
    public async Task Handle_ShouldExcludeInactiveShops()
    {
        var mallId = Guid.NewGuid();
        var category = Category.Create(mallId, "Coffee", "/icons/coffee.png", "Coffee shops", 1);

        var activeShop = Shop.Create(mallId, "Active Shop", category.CategoryId);
        AttachCategory(activeShop, category);

        var inactiveShop = Shop.Create(mallId, "Inactive Shop", category.CategoryId);
        inactiveShop.Deactivate();
        AttachCategory(inactiveShop, category);

        var stub = new ShopReadOnlyRepositoryStub([activeShop, inactiveShop]);
        var handler = new GetShops.Handler(stub);

        var result = await handler.Handle(
            new GetShops.Query(mallId, null, null, pageNumber: 1, pageSize: 10),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Data.Count.ShouldBe(1);
        result.Value.Data[0].ShopName.ShouldBe("Active Shop");
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
