using System.Linq.Expressions;
using System.Reflection;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Stamps.Command;
using Loop.Application.Stamps.Contract;
using Loop.Domain.QRCode;
using Loop.Domain.Shops;
using Loop.Domain.Stamps;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Loop.SharedKernel;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Stamps;

public class GenerateStampCollectQrCommandTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 4, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_ShouldFail_WhenStampsCountIsZeroOrNegative()
    {
        var qrCodeRepo = new QrCodeRepositoryStub([]);
        var stampReadRepo = new StampReadOnlyRepositoryStub([]);
        var context = new ShopAdminContextStub(Guid.NewGuid(), Guid.NewGuid());
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampCollectionQrTokenProviderStub();
        var handler = new GenerateStampCollectQr.Handler(qrCodeRepo, stampReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new GenerateStampCollectQr.Command(Guid.NewGuid(), 0),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StampErrors.InvalidQrPayload);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenStampNotFound()
    {
        var qrCodeRepo = new QrCodeRepositoryStub([]);
        var stampReadRepo = new StampReadOnlyRepositoryStub([]);
        var context = new ShopAdminContextStub(Guid.NewGuid(), Guid.NewGuid());
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampCollectionQrTokenProviderStub();
        var handler = new GenerateStampCollectQr.Handler(qrCodeRepo, stampReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new GenerateStampCollectQr.Command(Guid.NewGuid(), 1),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(StampErrors.NotFound(Guid.Empty).Code);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenValidRequest()
    {
        var shopId = Guid.NewGuid();
        var stamp = Stamp.Create(shopId, "Buy 5 Get 1", 5, StampType.Reward, FixedNow.AddDays(-10), FixedNow.AddDays(10));
        var qrCodeRepo = new QrCodeRepositoryStub([]);
        var stampReadRepo = new StampReadOnlyRepositoryStub([stamp]);
        var context = new ShopAdminContextStub(Guid.NewGuid(), shopId);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampCollectionQrTokenProviderStub();
        var handler = new GenerateStampCollectQr.Handler(qrCodeRepo, stampReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new GenerateStampCollectQr.Command(stamp.StampId, 1),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.QrId.ShouldNotBe(Guid.Empty);
        result.Value.ExpiresAtUtc.ShouldBe(FixedNow.AddMinutes(2));
    }

    private sealed class ShopAdminContextStub(Guid shopAdminId, Guid shopId) : IShopAdminContext
    {
        public Guid ShopAdminId { get; } = shopAdminId;
        public Guid ShopId { get; } = shopId;
    }

    private sealed class DateTimeProviderStub(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow => now;
    }

    private sealed class StampCollectionQrTokenProviderStub : IStampCollectionQrTokenProvider
    {
        public string CreateToken(StampCollectionQrTokenPayload payload) => "test-qr-token";
        public Task<StampCollectionQrTokenPayload?> ValidateAndGetPayloadAsync(string token) =>
            Task.FromResult<StampCollectionQrTokenPayload?>(null);
    }

    private sealed class QrCodeRepositoryStub(IEnumerable<QrCode> items) : IRepository<QrCode>
    {
        private readonly List<QrCode> _items = items.ToList();
        public IQueryable<QrCode> Find(ISpecification<QrCode> spec) => new TestAsyncEnumerable<QrCode>(_items);
        public Task<int> CountAsync(ISpecification<QrCode> spec) => Task.FromResult(_items.Count);
        public IQueryable<QrCode> GetAll() => new TestAsyncEnumerable<QrCode>(_items);
        public int Count(ISpecification<QrCode> spec) => _items.Count;
        public Task AddAsync(QrCode entity) { _items.Add(entity); return Task.CompletedTask; }
        public void Add(QrCode entity) => _items.Add(entity);
        public Task AddRangeAsync(IEnumerable<QrCode> entities) { _items.AddRange(entities); return Task.CompletedTask; }
        public void AddRange(IEnumerable<QrCode> entities) => _items.AddRange(entities);
        public Task DeleteAsync(QrCode entity) { _items.Remove(entity); return Task.CompletedTask; }
        public void Delete(QrCode entity) => _items.Remove(entity);
        public Task DeleteRangeAsync(IEnumerable<QrCode> entities) { foreach (var e in entities.ToList()) { _items.Remove(e); } return Task.CompletedTask; }
        public void DeleteRange(IEnumerable<QrCode> entities) { foreach (var e in entities.ToList()) { _items.Remove(e); } }
    }

    private sealed class StampReadOnlyRepositoryStub(IEnumerable<Stamp> items) : IReadOnlyRepository<Stamp>
    {
        private readonly List<Stamp> _items = items.ToList();
        public IQueryable<Stamp> Find(ISpecification<Stamp> spec) => new TestAsyncEnumerable<Stamp>(_items);
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
