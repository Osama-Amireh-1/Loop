using System.Linq.Expressions;
using System.Reflection;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Stamps.Query;
using Loop.Domain.Shops;
using Loop.Domain.Stamps;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Stamps;

public class GetUserStampCardsQueryTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 4, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_ShouldReturnUserStampCards_WhenCardsExist()
    {
        var userId = Guid.NewGuid();
        var shop = Shop.Create(Guid.NewGuid(), "Coffee Shop", Guid.NewGuid());
        var stamp = Stamp.Create(Guid.NewGuid(), "Buy 5 Get 1", 5, StampType.Reward, FixedNow.AddDays(-10), FixedNow.AddDays(10));
        AttachShop(stamp, shop);
        var card = UserStampCard.Open(userId, stamp.StampId);
        card.CollectStamp(5, 2);
        AttachStamp(card, stamp);

        var stub = new UserStampCardReadOnlyRepositoryStub([card]);
        var context = new UserContextStub(userId);
        var handler = new GetUserStampCards.Handler(stub, context);

        var result = await handler.Handle(new GetUserStampCards.Query(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(1);
        result.Value[0].stampId.ShouldBe(stamp.StampId);
        result.Value[0].shopName.ShouldBe("Coffee Shop");
        result.Value[0].stampsCounter.ShouldBe(2);
        result.Value[0].stampsRequired.ShouldBe(5);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoCards()
    {
        var stub = new UserStampCardReadOnlyRepositoryStub([]);
        var context = new UserContextStub(Guid.NewGuid());
        var handler = new GetUserStampCards.Handler(stub, context);

        var result = await handler.Handle(new GetUserStampCards.Query(), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    private static void AttachShop(Stamp stamp, Shop shop)
    {
        typeof(Stamp)
            .GetProperty(nameof(Stamp.Shop), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(stamp, shop);
    }

    private static void AttachStamp(UserStampCard card, Stamp stamp)
    {
        typeof(UserStampCard)
            .GetProperty(nameof(UserStampCard.Stamp), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?.SetValue(card, stamp);
    }

    private sealed class UserContextStub(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }

    private sealed class UserStampCardReadOnlyRepositoryStub(IEnumerable<UserStampCard> items) : IReadOnlyRepository<UserStampCard>
    {
        private readonly List<UserStampCard> _items = items.ToList();

        public IQueryable<UserStampCard> Find(ISpecification<UserStampCard> spec) =>
            new TestAsyncEnumerable<UserStampCard>(_items);

        public Task<int> CountAsync(ISpecification<UserStampCard> spec) => Task.FromResult(_items.Count);
        public IQueryable<UserStampCard> GetAll() => new TestAsyncEnumerable<UserStampCard>(_items);
        public int Count(ISpecification<UserStampCard> spec) => _items.Count;
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
