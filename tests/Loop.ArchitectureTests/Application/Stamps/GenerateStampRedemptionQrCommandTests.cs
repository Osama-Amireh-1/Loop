using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Stamps.Command;
using Loop.Application.Stamps.Contract;
using Loop.Domain.QRCode;
using Loop.Domain.Stamps;
using Loop.SharedKernel;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Stamps;

public class GenerateStampRedemptionQrCommandTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 4, 12, 0, 0, DateTimeKind.Utc);
    private const string TestToken = "test-redemption-token";

    [Fact]
    public async Task Handle_ShouldFail_WhenCardNotFound()
    {
        var userId = Guid.NewGuid();
        var qrCodeRepo = new QrCodeRepositoryStub([]);
        var userStampCardReadRepo = new UserStampCardReadOnlyRepositoryStub([]);
        var context = new UserContextStub(userId);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampRedemptionQrTokenProviderStub(null);

        var handler = new GenerateStampRedemptionQr.Handler(
            qrCodeRepo, userStampCardReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new GenerateStampRedemptionQr.Command(Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Stamps.CardNotFound");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCardNotCompleted()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var stamp = Stamp.Create(shopId, "Buy 5 Get 1", 5, StampType.Reward, FixedNow.AddDays(-10), FixedNow.AddDays(10));
        var card = UserStampCard.Open(userId, stamp.StampId);
        SetStampNavigation(card, stamp);

        var qrCodeRepo = new QrCodeRepositoryStub([]);
        var userStampCardReadRepo = new UserStampCardReadOnlyRepositoryStub([card]);
        var context = new UserContextStub(userId);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampRedemptionQrTokenProviderStub(null);

        var handler = new GenerateStampRedemptionQr.Handler(
            qrCodeRepo, userStampCardReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new GenerateStampRedemptionQr.Command(stamp.StampId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StampErrors.CardNotCompleted);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenCardIsCompleted()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var stamp = Stamp.Create(shopId, "Buy 5 Get 1", 5, StampType.Reward, FixedNow.AddDays(-10), FixedNow.AddDays(10));
        var card = UserStampCard.Open(userId, stamp.StampId);
        card.CollectStamp(stamp.StampsRequired, stamp.StampsRequired);
        card.IsCompleted.ShouldBeTrue();
        SetStampNavigation(card, stamp);

        var qrCodeRepo = new QrCodeRepositoryStub([]);
        var userStampCardReadRepo = new UserStampCardReadOnlyRepositoryStub([card]);
        var context = new UserContextStub(userId);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampRedemptionQrTokenProviderStub(null);

        var handler = new GenerateStampRedemptionQr.Handler(
            qrCodeRepo, userStampCardReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new GenerateStampRedemptionQr.Command(stamp.StampId),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.QrId.ShouldNotBe(Guid.Empty);
        result.Value.ExpiresAtUtc.ShouldBe(FixedNow.AddMinutes(2));
    }

    private static void SetStampNavigation(UserStampCard card, Stamp stamp)
    {
        typeof(UserStampCard)
            .GetProperty(nameof(UserStampCard.Stamp), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(card, stamp);
    }

    private sealed class UserContextStub(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }

    private sealed class DateTimeProviderStub(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow => now;
    }

    private sealed class StampRedemptionQrTokenProviderStub : IStampRedemptionQrTokenProvider
    {
        private readonly StampRedemptionQrTokenPayload? _payload;

        public StampRedemptionQrTokenProviderStub(StampRedemptionQrTokenPayload? payload)
        {
            _payload = payload;
        }

        public string CreateToken(StampRedemptionQrTokenPayload payload) => TestToken;

        public Task<StampRedemptionQrTokenPayload?> ValidateAndGetPayloadAsync(string token) =>
            Task.FromResult(_payload);
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

    private sealed class UserStampCardReadOnlyRepositoryStub(IEnumerable<UserStampCard> items) : IReadOnlyRepository<UserStampCard>
    {
        private readonly List<UserStampCard> _items = items.ToList();
        public IQueryable<UserStampCard> Find(ISpecification<UserStampCard> spec) => new TestAsyncEnumerable<UserStampCard>(_items);
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
