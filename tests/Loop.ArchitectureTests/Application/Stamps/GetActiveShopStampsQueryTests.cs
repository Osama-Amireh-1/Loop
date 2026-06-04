using System.Linq.Expressions;
using System.Reflection;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Stamps.Query;
using Loop.Domain.Shops;
using Loop.Domain.Stamps;
using Loop.Domain.Stamps.Specifications;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Stamps;

public class GetActiveShopStampsQueryTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 4, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_ShouldReturnActiveStamps_WhenStampsExist()
    {
        var shopId = Guid.NewGuid();
        var shop = Shop.Create(Guid.NewGuid(), "Test Shop", Guid.NewGuid());
        var stamp = Stamp.Create(shopId, "Buy 10 Get 1", 10, StampType.Reward, FixedNow.AddDays(-10), FixedNow.AddDays(10));
        AttachShop(stamp, shop);

        var stub = new StampReadOnlyRepositoryStub([stamp]);
        var context = new ShopAdminContextStub(Guid.NewGuid(), shopId);
        var handler = new GetActiveShopStamps.Handler(stub, context);

        var result = await handler.Handle(new GetActiveShopStamps.Query(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value[0].StampId.ShouldBe(stamp.StampId);
        result.Value[0].StampName.ShouldBe("Buy 10 Get 1");
        result.Value[0].StampsRequired.ShouldBe(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoActiveStamps()
    {
        var stub = new StampReadOnlyRepositoryStub([]);
        var context = new ShopAdminContextStub(Guid.NewGuid(), Guid.NewGuid());
        var handler = new GetActiveShopStamps.Handler(stub, context);

        var result = await handler.Handle(new GetActiveShopStamps.Query(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    private static void AttachShop(Stamp stamp, Shop shop)
    {
        typeof(Stamp)
            .GetProperty(nameof(Stamp.Shop), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(stamp, shop);
    }

    private sealed class ShopAdminContextStub(Guid shopAdminId, Guid shopId) : IShopAdminContext
    {
        public Guid ShopAdminId { get; } = shopAdminId;
        public Guid ShopId { get; } = shopId;
    }

    private sealed class StampReadOnlyRepositoryStub(IEnumerable<Stamp> items) : IReadOnlyRepository<Stamp>
    {
        private readonly List<Stamp> _items = items.ToList();

        public IQueryable<Stamp> Find(ISpecification<Stamp> spec)
        {
            IQueryable<Stamp> query = _items.AsQueryable();
            if (spec.Criteria is not null)
            {
                query = query.Where(spec.Criteria.Compile()).AsQueryable();
            }
            return new TestAsyncEnumerable<Stamp>(query);
        }

        public Task<int> CountAsync(ISpecification<Stamp> spec) => Task.FromResult(_items.Count);
        public IQueryable<Stamp> GetAll() => new TestAsyncEnumerable<Stamp>(_items);
        public int Count(ISpecification<Stamp> spec) => _items.Count;
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
        public TestAsyncEnumerable(Expression expression) : this(new EnumerableQuery<T>(expression)) { }
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;
        public ValueTask DisposeAsync() { inner.Dispose(); return ValueTask.CompletedTask; }
        public ValueTask<bool> MoveNextAsync() => new(inner.MoveNext());
    }
}
