using System.Linq.Expressions;
using System.Text.Json;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Stamps.Command;
using Loop.Domain.QRCode;
using Loop.Domain.QRCode.Specifications;
using Loop.Domain.Stamps;
using Loop.SharedKernel;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Stamps;

public class ConfirmStampRedemptionQrCommandTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 4, 12, 0, 0, DateTimeKind.Utc);
    private const string TestToken = "test-redemption-token";

    [Fact]
    public async Task Handle_ShouldFail_WhenQrCodeNotFound()
    {
        var qrCodeRepo = new QrCodeRepositoryStub([]);
        var userStampCardRepo = new UserStampCardRepositoryStub([]);
        var stampRedemptionRepo = new StampRedemptionRepositoryStub([]);
        var stampRedemptionReadRepo = new StampRedemptionReadOnlyRepositoryStub([]);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampRedemptionQrTokenProviderStub(null);
        var shopAdminContext = new ShopAdminContextStub(Guid.NewGuid());

        var handler = new ConfirmStampRedemptionQr.Handler(
            qrCodeRepo, userStampCardRepo, stampRedemptionRepo, stampRedemptionReadRepo,
            dateTimeProvider, tokenProvider, shopAdminContext);

        var result = await handler.Handle(
            new ConfirmStampRedemptionQr.Command(Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StampErrors.QrCodeNotFound);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenQrPayloadIsInvalid()
    {
        var shopId = Guid.NewGuid();
        var qrCode = QrCode.Create(null, shopId, JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(5));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);
        var userStampCardRepo = new UserStampCardRepositoryStub([]);
        var stampRedemptionRepo = new StampRedemptionRepositoryStub([]);
        var stampRedemptionReadRepo = new StampRedemptionReadOnlyRepositoryStub([]);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new StampRedemptionQrTokenProviderStub(null);
        var shopAdminContext = new ShopAdminContextStub(shopId);

        var handler = new ConfirmStampRedemptionQr.Handler(
            qrCodeRepo, userStampCardRepo, stampRedemptionRepo, stampRedemptionReadRepo,
            dateTimeProvider, tokenProvider, shopAdminContext);

        var result = await handler.Handle(
            new ConfirmStampRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StampErrors.InvalidQrPayload);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenShopIdMismatch()
    {
        var qrCode = QrCode.Create(null, Guid.NewGuid(), JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(5));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);
        var userStampCardRepo = new UserStampCardRepositoryStub([]);
        var stampRedemptionRepo = new StampRedemptionRepositoryStub([]);
        var stampRedemptionReadRepo = new StampRedemptionReadOnlyRepositoryStub([]);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);

        var differentShopId = Guid.NewGuid();
        var payload = new StampRedemptionQrTokenPayload("tid", Guid.NewGuid(), Guid.NewGuid(), differentShopId, FixedNow.AddMinutes(5));
        var tokenProvider = new StampRedemptionQrTokenProviderStub(payload);
        var shopAdminContext = new ShopAdminContextStub(Guid.NewGuid());

        var handler = new ConfirmStampRedemptionQr.Handler(
            qrCodeRepo, userStampCardRepo, stampRedemptionRepo, stampRedemptionReadRepo,
            dateTimeProvider, tokenProvider, shopAdminContext);

        var result = await handler.Handle(
            new ConfirmStampRedemptionQr.Command(qrCode.QrId),
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
        var userStampCardRepo = new UserStampCardRepositoryStub([]);
        var stampRedemptionRepo = new StampRedemptionRepositoryStub([]);
        var stampRedemptionReadRepo = new StampRedemptionReadOnlyRepositoryStub([]);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);

        var expiredPayload = new StampRedemptionQrTokenPayload(
            "tid", Guid.NewGuid(), Guid.NewGuid(), shopId, FixedNow.AddMinutes(-5));
        var tokenProvider = new StampRedemptionQrTokenProviderStub(expiredPayload);
        var shopAdminContext = new ShopAdminContextStub(shopId);

        var handler = new ConfirmStampRedemptionQr.Handler(
            qrCodeRepo, userStampCardRepo, stampRedemptionRepo, stampRedemptionReadRepo,
            dateTimeProvider, tokenProvider, shopAdminContext);

        var result = await handler.Handle(
            new ConfirmStampRedemptionQr.Command(qrCode.QrId),
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
        var userStampCardRepo = new UserStampCardRepositoryStub([]);

        var existingRedemption = StampRedemption.Create(Guid.NewGuid(), shopId, Guid.NewGuid(), qrCode.QrId);
        var stampRedemptionRepo = new StampRedemptionRepositoryStub([]);
        var stampRedemptionReadRepo = new StampRedemptionReadOnlyRepositoryStub([existingRedemption]);

        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var payload = new StampRedemptionQrTokenPayload(
            "tid", Guid.NewGuid(), Guid.NewGuid(), shopId, FixedNow.AddMinutes(5));
        var tokenProvider = new StampRedemptionQrTokenProviderStub(payload);
        var shopAdminContext = new ShopAdminContextStub(shopId);

        var handler = new ConfirmStampRedemptionQr.Handler(
            qrCodeRepo, userStampCardRepo, stampRedemptionRepo, stampRedemptionReadRepo,
            dateTimeProvider, tokenProvider, shopAdminContext);

        var result = await handler.Handle(
            new ConfirmStampRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StampErrors.QrCodeAlreadyUsed);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCardNotFound()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var stampId = Guid.NewGuid();
        var qrCode = QrCode.Create(userId, shopId, JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(5));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);
        var userStampCardRepo = new UserStampCardRepositoryStub([]);
        var stampRedemptionRepo = new StampRedemptionRepositoryStub([]);
        var stampRedemptionReadRepo = new StampRedemptionReadOnlyRepositoryStub([]);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);

        var payload = new StampRedemptionQrTokenPayload(
            "tid", stampId, userId, shopId, FixedNow.AddMinutes(5));
        var tokenProvider = new StampRedemptionQrTokenProviderStub(payload);
        var shopAdminContext = new ShopAdminContextStub(shopId);

        var handler = new ConfirmStampRedemptionQr.Handler(
            qrCodeRepo, userStampCardRepo, stampRedemptionRepo, stampRedemptionReadRepo,
            dateTimeProvider, tokenProvider, shopAdminContext);

        var result = await handler.Handle(
            new ConfirmStampRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(StampErrors.CardNotFound(userId, stampId).Code);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCardNotCompleted()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var stamp = Stamp.Create(shopId, "Buy 5 Get 1", 5, StampType.Reward, FixedNow.AddDays(-10), FixedNow.AddDays(10));
        var card = UserStampCard.Open(userId, stamp.StampId);
        var qrCode = QrCode.Create(userId, shopId, JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(5));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);
        var userStampCardRepo = new UserStampCardRepositoryStub([card]);
        var stampRedemptionRepo = new StampRedemptionRepositoryStub([]);
        var stampRedemptionReadRepo = new StampRedemptionReadOnlyRepositoryStub([]);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);

        var payload = new StampRedemptionQrTokenPayload(
            "tid", stamp.StampId, userId, shopId, FixedNow.AddMinutes(5));
        var tokenProvider = new StampRedemptionQrTokenProviderStub(payload);
        var shopAdminContext = new ShopAdminContextStub(shopId);

        var handler = new ConfirmStampRedemptionQr.Handler(
            qrCodeRepo, userStampCardRepo, stampRedemptionRepo, stampRedemptionReadRepo,
            dateTimeProvider, tokenProvider, shopAdminContext);

        var result = await handler.Handle(
            new ConfirmStampRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(StampErrors.CardNotCompleted);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenValid()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        var stamp = Stamp.Create(shopId, "Buy 5 Get 1", 5, StampType.Reward, FixedNow.AddDays(-10), FixedNow.AddDays(10));
        var card = UserStampCard.Open(userId, stamp.StampId);
        card.CollectStamp(stamp.StampsRequired, stamp.StampsRequired);
        card.IsCompleted.ShouldBeTrue();
        var qrCode = QrCode.Create(userId, shopId, JsonSerializer.Serialize(TestToken), FixedNow.AddMinutes(5));
        var qrCodeRepo = new QrCodeRepositoryStub([qrCode]);
        var userStampCardRepo = new UserStampCardRepositoryStub([card]);
        var stampRedemptionRepo = new StampRedemptionRepositoryStub([]);
        var stampRedemptionReadRepo = new StampRedemptionReadOnlyRepositoryStub([]);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);

        var payload = new StampRedemptionQrTokenPayload(
            "tid", stamp.StampId, userId, shopId, FixedNow.AddMinutes(5));
        var tokenProvider = new StampRedemptionQrTokenProviderStub(payload);
        var shopAdminContext = new ShopAdminContextStub(shopId);

        var handler = new ConfirmStampRedemptionQr.Handler(
            qrCodeRepo, userStampCardRepo, stampRedemptionRepo, stampRedemptionReadRepo,
            dateTimeProvider, tokenProvider, shopAdminContext);

        var result = await handler.Handle(
            new ConfirmStampRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    private sealed class ShopAdminContextStub(Guid shopId) : IShopAdminContext
    {
        public Guid ShopAdminId { get; } = Guid.NewGuid();
        public Guid ShopId { get; } = shopId;
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

    private sealed class StampRedemptionRepositoryStub(IEnumerable<StampRedemption> items) : IRepository<StampRedemption>
    {
        private readonly List<StampRedemption> _items = items.ToList();
        public IQueryable<StampRedemption> Find(ISpecification<StampRedemption> spec) => new TestAsyncEnumerable<StampRedemption>(_items);
        public Task<int> CountAsync(ISpecification<StampRedemption> spec) => Task.FromResult(_items.Count);
        public IQueryable<StampRedemption> GetAll() => new TestAsyncEnumerable<StampRedemption>(_items);
        public int Count(ISpecification<StampRedemption> spec) => _items.Count;
        public Task AddAsync(StampRedemption entity) { _items.Add(entity); return Task.CompletedTask; }
        public void Add(StampRedemption entity) => _items.Add(entity);
        public Task AddRangeAsync(IEnumerable<StampRedemption> entities) { _items.AddRange(entities); return Task.CompletedTask; }
        public void AddRange(IEnumerable<StampRedemption> entities) => _items.AddRange(entities);
        public Task DeleteAsync(StampRedemption entity) { _items.Remove(entity); return Task.CompletedTask; }
        public void Delete(StampRedemption entity) => _items.Remove(entity);
        public Task DeleteRangeAsync(IEnumerable<StampRedemption> entities) { foreach (var e in entities.ToList()) { _items.Remove(e); } return Task.CompletedTask; }
        public void DeleteRange(IEnumerable<StampRedemption> entities) { foreach (var e in entities.ToList()) { _items.Remove(e); } }
    }

    private sealed class StampRedemptionReadOnlyRepositoryStub(IEnumerable<StampRedemption> items) : IReadOnlyRepository<StampRedemption>
    {
        private readonly List<StampRedemption> _items = items.ToList();
        public IQueryable<StampRedemption> Find(ISpecification<StampRedemption> spec) => new TestAsyncEnumerable<StampRedemption>(_items);
        public Task<int> CountAsync(ISpecification<StampRedemption> spec) => Task.FromResult(_items.Count);
        public IQueryable<StampRedemption> GetAll() => new TestAsyncEnumerable<StampRedemption>(_items);
        public int Count(ISpecification<StampRedemption> spec) => _items.Count;
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
