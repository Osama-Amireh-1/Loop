using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
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

public class ScanStampCollectQrCommandTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 4, 12, 0, 0, DateTimeKind.Utc);
    private const string TestToken = "test-collect-token";

    [Fact]
    public async Task Handle_ShouldFail_WhenQrCodeNotFound()
    {
        var qrCodeRepo = new QrCodeRepositoryStub([]);
        var stampReadRepo = new StampReadOnlyRepositoryStub([]);
        var userStampCardRepo = new UserStampCardRepositoryStub([]);
        var stampTxRepo = new StampTransactionRepositoryStub([]);
        var stampTxReadRepo = new StampTransactionReadOnlyRepositoryStub([]);
        var context = new UserContextStub(Guid.NewGuid());
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampCollectionQrTokenProviderStub(null);

        var handler = new ScanStampCollectQr.Handler(
            qrCodeRepo, userStampCardRepo, stampTxRepo, stampTxReadRepo,
            stampReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new ScanStampCollectQr.Command(Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StampErrors.QrCodeNotFound);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenQrPayloadIsInvalid()
    {
        var shopId = Guid.NewGuid();
        var qrCode = QrCode.Create(null, shopId, JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(2));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);
        var stampReadRepo = new StampReadOnlyRepositoryStub([]);
        var userStampCardRepo = new UserStampCardRepositoryStub([]);
        var stampTxRepo = new StampTransactionRepositoryStub([]);
        var stampTxReadRepo = new StampTransactionReadOnlyRepositoryStub([]);
        var context = new UserContextStub(Guid.NewGuid());
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampCollectionQrTokenProviderStub(null);

        var handler = new ScanStampCollectQr.Handler(
            qrCodeRepo, userStampCardRepo, stampTxRepo, stampTxReadRepo,
            stampReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new ScanStampCollectQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StampErrors.InvalidQrPayload);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenQrCodeExpired()
    {
        var shopId = Guid.NewGuid();
        var qrCode = QrCode.Create(null, shopId, JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(-5));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);
        var stampReadRepo = new StampReadOnlyRepositoryStub([]);
        var userStampCardRepo = new UserStampCardRepositoryStub([]);
        var stampTxRepo = new StampTransactionRepositoryStub([]);
        var stampTxReadRepo = new StampTransactionReadOnlyRepositoryStub([]);
        var context = new UserContextStub(Guid.NewGuid());

        var expiredPayload = new StampCollectionQrTokenPayload(
            "tid", Guid.NewGuid(), shopId, 1, FixedNow.AddMinutes(-5));
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampCollectionQrTokenProviderStub(expiredPayload);

        var handler = new ScanStampCollectQr.Handler(
            qrCodeRepo, userStampCardRepo, stampTxRepo, stampTxReadRepo,
            stampReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new ScanStampCollectQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StampErrors.QrCodeExpired);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenQrCodeAlreadyUsed()
    {
        var shopId = Guid.NewGuid();
        var qrCode = QrCode.Create(null, shopId, JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(5));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);

        var stamp = Stamp.Create(shopId, "Buy 5 Get 1", 5, StampType.Reward, FixedNow.AddDays(-10), FixedNow.AddDays(10));
        var stampReadRepo = new StampReadOnlyRepositoryStub([stamp]);

        var userStampCardRepo = new UserStampCardRepositoryStub([]);
        var stampTxRepo = new StampTransactionRepositoryStub([]);
        var stampTxReadRepo = new StampTransactionReadOnlyRepositoryStub([StampTransaction.RecordCollect(
            Guid.NewGuid(), shopId, stamp.StampId, 1, qrCode.QrId)]);

        var context = new UserContextStub(Guid.NewGuid());

        var payload = new StampCollectionQrTokenPayload(
            "tid", stamp.StampId, shopId, 1, FixedNow.AddMinutes(5));
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampCollectionQrTokenProviderStub(payload);

        var handler = new ScanStampCollectQr.Handler(
            qrCodeRepo, userStampCardRepo, stampTxRepo, stampTxReadRepo,
            stampReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new ScanStampCollectQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StampErrors.QrCodeAlreadyUsed);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenStampNotFound()
    {
        var shopId = Guid.NewGuid();
        var qrCode = QrCode.Create(null, shopId, JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(5));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);
        var stampReadRepo = new StampReadOnlyRepositoryStub([]);
        var userStampCardRepo = new UserStampCardRepositoryStub([]);
        var stampTxRepo = new StampTransactionRepositoryStub([]);
        var stampTxReadRepo = new StampTransactionReadOnlyRepositoryStub([]);
        var context = new UserContextStub(Guid.NewGuid());

        var payload = new StampCollectionQrTokenPayload(
            "tid", Guid.NewGuid(), shopId, 1, FixedNow.AddMinutes(5));
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampCollectionQrTokenProviderStub(payload);

        var handler = new ScanStampCollectQr.Handler(
            qrCodeRepo, userStampCardRepo, stampTxRepo, stampTxReadRepo,
            stampReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new ScanStampCollectQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(StampErrors.NotFound(Guid.Empty).Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenStampIsInactive()
    {
        var shopId = Guid.NewGuid();
        var qrCode = QrCode.Create(null, shopId, JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(5));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);

        var stamp = Stamp.Create(shopId, "Buy 5 Get 1", 5, StampType.Reward, FixedNow.AddDays(-10), FixedNow.AddDays(10));
        stamp.Deactivate();
        var stampReadRepo = new StampReadOnlyRepositoryStub([stamp]);

        var userStampCardRepo = new UserStampCardRepositoryStub([]);
        var stampTxRepo = new StampTransactionRepositoryStub([]);
        var stampTxReadRepo = new StampTransactionReadOnlyRepositoryStub([]);
        var context = new UserContextStub(Guid.NewGuid());

        var payload = new StampCollectionQrTokenPayload(
            "tid", stamp.StampId, shopId, 1, FixedNow.AddMinutes(5));
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampCollectionQrTokenProviderStub(payload);

        var handler = new ScanStampCollectQr.Handler(
            qrCodeRepo, userStampCardRepo, stampTxRepo, stampTxReadRepo,
            stampReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new ScanStampCollectQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(StampErrors.NotFound(stamp.StampId).Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCardAlreadyCompleted()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var qrCode = QrCode.Create(null, shopId, JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(5));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);

        var stamp = Stamp.Create(shopId, "Buy 5 Get 1", 5, StampType.Reward, FixedNow.AddDays(-10), FixedNow.AddDays(10));
        var stampReadRepo = new StampReadOnlyRepositoryStub([stamp]);

        var card = UserStampCard.Open(userId, stamp.StampId);
        card.CollectStamp(5, 5);
        card.IsCompleted.ShouldBeTrue();
        var userStampCardRepo = new UserStampCardRepositoryStub([card]);

        var stampTxRepo = new StampTransactionRepositoryStub([]);
        var stampTxReadRepo = new StampTransactionReadOnlyRepositoryStub([]);
        var context = new UserContextStub(userId);

        var payload = new StampCollectionQrTokenPayload(
            "tid", stamp.StampId, shopId, 1, FixedNow.AddMinutes(5));
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampCollectionQrTokenProviderStub(payload);

        var handler = new ScanStampCollectQr.Handler(
            qrCodeRepo, userStampCardRepo, stampTxRepo, stampTxReadRepo,
            stampReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new ScanStampCollectQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StampErrors.CardAlreadyCompleted);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_CreatingNewCard()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var qrCode = QrCode.Create(null, shopId, JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(5));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);

        var stamp = Stamp.Create(shopId, "Buy 5 Get 1", 5, StampType.Reward, FixedNow.AddDays(-10), FixedNow.AddDays(10));
        var stampReadRepo = new StampReadOnlyRepositoryStub([stamp]);

        var userStampCardRepo = new UserStampCardRepositoryStub([]);
        var stampTxRepo = new StampTransactionRepositoryStub([]);
        var stampTxReadRepo = new StampTransactionReadOnlyRepositoryStub([]);
        var context = new UserContextStub(userId);

        var payload = new StampCollectionQrTokenPayload(
            "tid", stamp.StampId, shopId, 1, FixedNow.AddMinutes(5));
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampCollectionQrTokenProviderStub(payload);

        var handler = new ScanStampCollectQr.Handler(
            qrCodeRepo, userStampCardRepo, stampTxRepo, stampTxReadRepo,
            stampReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new ScanStampCollectQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.StampId.ShouldBe(stamp.StampId);
        result.Value.StampsCounter.ShouldBe(1);
        result.Value.IsCompleted.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WithExistingCard()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var qrCode = QrCode.Create(null, shopId, JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(5));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);

        var stamp = Stamp.Create(shopId, "Buy 5 Get 1", 5, StampType.Reward, FixedNow.AddDays(-10), FixedNow.AddDays(10));
        var stampReadRepo = new StampReadOnlyRepositoryStub([stamp]);

        var card = UserStampCard.Open(userId, stamp.StampId);
        card.CollectStamp(5, 2);
        var userStampCardRepo = new UserStampCardRepositoryStub([card]);

        var stampTxRepo = new StampTransactionRepositoryStub([]);
        var stampTxReadRepo = new StampTransactionReadOnlyRepositoryStub([]);
        var context = new UserContextStub(userId);

        var payload = new StampCollectionQrTokenPayload(
            "tid", stamp.StampId, shopId, 1, FixedNow.AddMinutes(5));
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampCollectionQrTokenProviderStub(payload);

        var handler = new ScanStampCollectQr.Handler(
            qrCodeRepo, userStampCardRepo, stampTxRepo, stampTxReadRepo,
            stampReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new ScanStampCollectQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.StampId.ShouldBe(stamp.StampId);
        result.Value.StampsCounter.ShouldBe(3);
        result.Value.IsCompleted.ShouldBeFalse();
    }

    private sealed class UserContextStub(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }

    private sealed class DateTimeProviderStub(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow => now;
    }

    private sealed class StampCollectionQrTokenProviderStub : IStampCollectionQrTokenProvider
    {
        private readonly StampCollectionQrTokenPayload? _payload;

        public StampCollectionQrTokenProviderStub(StampCollectionQrTokenPayload? payload)
        {
            _payload = payload;
        }

        public string CreateToken(StampCollectionQrTokenPayload payload) => TestToken;

        public Task<StampCollectionQrTokenPayload?> ValidateAndGetPayloadAsync(string token) =>
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

    private sealed class StampReadOnlyRepositoryStub(IEnumerable<Stamp> items) : IReadOnlyRepository<Stamp>
    {
        private readonly List<Stamp> _items = items.ToList();
        public IQueryable<Stamp> Find(ISpecification<Stamp> spec) =>
            new TestAsyncEnumerable<Stamp>(
                spec.Criteria is not null
                    ? _items.Where(spec.Criteria.Compile()).ToList()
                    : _items);
        public Task<int> CountAsync(ISpecification<Stamp> spec) => Task.FromResult(_items.Count);
        public IQueryable<Stamp> GetAll() => new TestAsyncEnumerable<Stamp>(_items);
        public int Count(ISpecification<Stamp> spec) => _items.Count;
    }

    private sealed class UserStampCardRepositoryStub(IEnumerable<UserStampCard> items) : IRepository<UserStampCard>
    {
        private readonly List<UserStampCard> _items = items.ToList();
        public IQueryable<UserStampCard> Find(ISpecification<UserStampCard> spec) => new TestAsyncEnumerable<UserStampCard>(_items);
        public Task<int> CountAsync(ISpecification<UserStampCard> spec) => Task.FromResult(_items.Count);
        public IQueryable<UserStampCard> GetAll() => new TestAsyncEnumerable<UserStampCard>(_items);
        public int Count(ISpecification<UserStampCard> spec) => _items.Count;
        public Task AddAsync(UserStampCard entity) { _items.Add(entity); return Task.CompletedTask; }
        public void Add(UserStampCard entity) => _items.Add(entity);
        public Task AddRangeAsync(IEnumerable<UserStampCard> entities) { _items.AddRange(entities); return Task.CompletedTask; }
        public void AddRange(IEnumerable<UserStampCard> entities) => _items.AddRange(entities);
        public Task DeleteAsync(UserStampCard entity) { _items.Remove(entity); return Task.CompletedTask; }
        public void Delete(UserStampCard entity) => _items.Remove(entity);
        public Task DeleteRangeAsync(IEnumerable<UserStampCard> entities) { foreach (var e in entities.ToList()) { _items.Remove(e); } return Task.CompletedTask; }
        public void DeleteRange(IEnumerable<UserStampCard> entities) { foreach (var e in entities.ToList()) { _items.Remove(e); } }
    }

    private sealed class StampTransactionRepositoryStub(IEnumerable<StampTransaction> items) : IRepository<StampTransaction>
    {
        private readonly List<StampTransaction> _items = items.ToList();
        public IQueryable<StampTransaction> Find(ISpecification<StampTransaction> spec) => new TestAsyncEnumerable<StampTransaction>(_items);
        public Task<int> CountAsync(ISpecification<StampTransaction> spec) => Task.FromResult(_items.Count);
        public IQueryable<StampTransaction> GetAll() => new TestAsyncEnumerable<StampTransaction>(_items);
        public int Count(ISpecification<StampTransaction> spec) => _items.Count;
        public Task AddAsync(StampTransaction entity) { _items.Add(entity); return Task.CompletedTask; }
        public void Add(StampTransaction entity) => _items.Add(entity);
        public Task AddRangeAsync(IEnumerable<StampTransaction> entities) { _items.AddRange(entities); return Task.CompletedTask; }
        public void AddRange(IEnumerable<StampTransaction> entities) => _items.AddRange(entities);
        public Task DeleteAsync(StampTransaction entity) { _items.Remove(entity); return Task.CompletedTask; }
        public void Delete(StampTransaction entity) => _items.Remove(entity);
        public Task DeleteRangeAsync(IEnumerable<StampTransaction> entities) { foreach (var e in entities.ToList()) { _items.Remove(e); } return Task.CompletedTask; }
        public void DeleteRange(IEnumerable<StampTransaction> entities) { foreach (var e in entities.ToList()) { _items.Remove(e); } }
    }

    private sealed class StampTransactionReadOnlyRepositoryStub(IEnumerable<StampTransaction> items) : IReadOnlyRepository<StampTransaction>
    {
        private readonly List<StampTransaction> _items = items.ToList();
        public IQueryable<StampTransaction> Find(ISpecification<StampTransaction> spec) => new TestAsyncEnumerable<StampTransaction>(_items);
        public Task<int> CountAsync(ISpecification<StampTransaction> spec) => Task.FromResult(_items.Count);
        public IQueryable<StampTransaction> GetAll() => new TestAsyncEnumerable<StampTransaction>(_items);
        public int Count(ISpecification<StampTransaction> spec) => _items.Count;
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
