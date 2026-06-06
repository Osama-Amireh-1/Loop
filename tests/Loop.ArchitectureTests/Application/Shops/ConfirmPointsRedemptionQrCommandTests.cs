using System.Linq.Expressions;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Shops.Command;
using Loop.Domain.Audit;
using Loop.Domain.Common;
using Loop.Domain.Configuration;
using Loop.Domain.QRCode;
using Loop.Domain.Shops;
using Loop.Domain.Transactions;
using Loop.Domain.Users;
using Loop.SharedKernel;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Shops;

public class ConfirmPointsRedemptionQrCommandTests
{
    private static readonly DateTime FixedUtcNow = new(2026, 6, 4, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_ShouldReturnQrCodeNotFound_WhenQrCodeDoesNotExist()
    {
        var stubs = new HandlerStubs();

        var handler = CreateHandler(stubs);

        var result = await handler.Handle(
            new ConfirmPointsRedemptionQr.Command(Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Transactions.QrCodeNotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidQrPayload_WhenPayloadIsNull()
    {
        var stubs = new HandlerStubs();
        var qrCode = QrCode.Create(Guid.NewGuid(), Guid.NewGuid(), "\"token\"", FixedUtcNow.AddHours(1));
        stubs.QrCodes = [qrCode];
        stubs.ValidatePayloadResult = null;

        var handler = CreateHandler(stubs);

        var result = await handler.Handle(
            new ConfirmPointsRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Transactions.InvalidQrPayload");
    }


    [Fact]
    public async Task Handle_ShouldReturnInvalidQrPayload_WhenShopIdMismatch()
    {
        var stubs = new HandlerStubs();
        var userId = Guid.NewGuid();
        var shopA = Shop.Create(Guid.NewGuid(), "Shop A", Guid.NewGuid());
        var qrCode = QrCode.Create(userId, shopA.ShopId, "\"token\"", FixedUtcNow.AddHours(1));
        stubs.QrCodes = [qrCode];
        stubs.ValidatePayloadResult = new PointsRedemptionQrTokenPayload("tok", userId, 50, FixedUtcNow.AddHours(1));
        stubs.ShopAdminContext = new ShopAdminContextStub(Guid.NewGuid(), Guid.NewGuid());

        var handler = CreateHandler(stubs);

        var result = await handler.Handle(
            new ConfirmPointsRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Transactions.InvalidQrPayload");
    }

    [Fact]
    public async Task Handle_ShouldReturnQrCodeExpired_WhenPayloadIsExpired()
    {
        var stubs = new HandlerStubs();
        var userId = Guid.NewGuid();
        var qrCode = QrCode.Create(userId, stubs.ShopAdminContext.ShopId, "\"token\"", FixedUtcNow.AddHours(1));
        stubs.QrCodes = [qrCode];
        stubs.ValidatePayloadResult = new PointsRedemptionQrTokenPayload("tok", userId, 50, FixedUtcNow.AddHours(-1));

        var handler = CreateHandler(stubs);

        var result = await handler.Handle(
            new ConfirmPointsRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Transactions.QrCodeExpired");
    }

    [Fact]
    public async Task Handle_ShouldReturnShopNotFound_WhenShopDoesNotExist()
    {
        var stubs = new HandlerStubs();
        var userId = Guid.NewGuid();
        var qrCode = QrCode.Create(userId, stubs.ShopAdminContext.ShopId, "\"token\"", FixedUtcNow.AddHours(1));
        stubs.QrCodes = [qrCode];
        stubs.ValidatePayloadResult = new PointsRedemptionQrTokenPayload("tok", userId, 50, FixedUtcNow.AddHours(1));

        var handler = CreateHandler(stubs);

        var result = await handler.Handle(
            new ConfirmPointsRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Shops.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnConfigNotFound_WhenSystemConfigDoesNotExist()
    {
        var stubs = new HandlerStubs();
        var userId = Guid.NewGuid();
        var mallId = Guid.NewGuid();
        var shop = Shop.Create(mallId, "Test Shop", Guid.NewGuid());
        var qrCode = QrCode.Create(userId, shop.ShopId, "\"token\"", FixedUtcNow.AddHours(1));
        stubs.QrCodes = [qrCode];
        stubs.ValidatePayloadResult = new PointsRedemptionQrTokenPayload("tok", userId, 50, FixedUtcNow.AddHours(1));
        stubs.ShopAdminContext = new ShopAdminContextStub(Guid.NewGuid(), shop.ShopId);
        stubs.Shops = [shop];

        var handler = CreateHandler(stubs);

        var result = await handler.Handle(
            new ConfirmPointsRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Configuration.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnUserNotFound_WhenUserDoesNotExist()
    {
        var stubs = new HandlerStubs();
        var userId = Guid.NewGuid();
        var mallId = Guid.NewGuid();
        var shop = Shop.Create(mallId, "Test Shop", Guid.NewGuid());
        var qrCode = QrCode.Create(userId, shop.ShopId, "\"token\"", FixedUtcNow.AddHours(1));
        stubs.QrCodes = [qrCode];
        stubs.ValidatePayloadResult = new PointsRedemptionQrTokenPayload("tok", userId, 50, FixedUtcNow.AddHours(1));
        stubs.ShopAdminContext = new ShopAdminContextStub(Guid.NewGuid(), shop.ShopId);
        stubs.Shops = [shop];
        stubs.SystemConfigs = [SystemConfig.Create(mallId, Guid.NewGuid(), 0.5m, 3)];

        var handler = CreateHandler(stubs);

        var result = await handler.Handle(
            new ConfirmPointsRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnBelowMinThreshold_WhenUserPointsAreBelowMinRedemptionThreshold()
    {
        var stubs = new HandlerStubs();
        var mallId = Guid.NewGuid();
        var shop = Shop.Create(mallId, "Test Shop", Guid.NewGuid());

        var phone = Phone.Create("1234567").Value;
        var email = Email.Create("user@test.com").Value;
        var user = User.Create("John", "Doe", phone, email, "hash", Gender.Male, mallId);
        user.CreditPoints(50);

        var qrCode = QrCode.Create(user.UserId, shop.ShopId, "\"token\"", FixedUtcNow.AddHours(1));
        stubs.QrCodes = [qrCode];
        stubs.ValidatePayloadResult = new PointsRedemptionQrTokenPayload("tok", user.UserId, 50, FixedUtcNow.AddHours(1));
        stubs.ShopAdminContext = new ShopAdminContextStub(Guid.NewGuid(), shop.ShopId);
        stubs.Shops = [shop];

        var config = SystemConfig.Create(mallId, Guid.NewGuid(), 0.5m, 3);
        config.Update(0.5m, 3, 100, Guid.NewGuid());
        stubs.SystemConfigs = [config];
        stubs.Users = [user];

        var handler = CreateHandler(stubs);

        var result = await handler.Handle(
            new ConfirmPointsRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Transactions.BelowMinRedemptionThreshold");
    }

    [Fact]
    public async Task Handle_ShouldReturnInsufficientPoints_WhenUserDoesNotHaveEnoughPoints()
    {
        var stubs = new HandlerStubs();
        var mallId = Guid.NewGuid();
        var shop = Shop.Create(mallId, "Test Shop", Guid.NewGuid());

        var phone = Phone.Create("1234567").Value;
        var email = Email.Create("user@test.com").Value;
        var user = User.Create("John", "Doe", phone, email, "hash", Gender.Male, mallId);
        user.CreditPoints(50);

        var qrCode = QrCode.Create(user.UserId, shop.ShopId, "\"token\"", FixedUtcNow.AddHours(1));
        stubs.QrCodes = [qrCode];
        stubs.ValidatePayloadResult = new PointsRedemptionQrTokenPayload("tok", user.UserId, 500, FixedUtcNow.AddHours(1));
        stubs.ShopAdminContext = new ShopAdminContextStub(Guid.NewGuid(), shop.ShopId);
        stubs.Shops = [shop];

        var config = SystemConfig.Create(mallId, Guid.NewGuid(), 0.5m, 3);
        config.Update(0.5m, 3, 10, Guid.NewGuid());
        stubs.SystemConfigs = [config];
        stubs.Users = [user];

        var handler = CreateHandler(stubs);

        var result = await handler.Handle(
            new ConfirmPointsRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Transactions.InsufficientPoints");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenAllConditionsAreMet()
    {
        var stubs = new HandlerStubs();
        var mallId = Guid.NewGuid();
        var shop = Shop.Create(mallId, "Test Shop", Guid.NewGuid());

        var phone = Phone.Create("1234567").Value;
        var email = Email.Create("user@test.com").Value;
        var user = User.Create("John", "Doe", phone, email, "hash", Gender.Male, mallId);
        user.CreditPoints(200);

        var qrCode = QrCode.Create(user.UserId, shop.ShopId, "\"token\"", FixedUtcNow.AddHours(1));
        stubs.QrCodes = [qrCode];
        stubs.ValidatePayloadResult = new PointsRedemptionQrTokenPayload("tok", user.UserId, 50, FixedUtcNow.AddHours(1));
        stubs.ShopAdminContext = new ShopAdminContextStub(Guid.NewGuid(), shop.ShopId);
        stubs.Shops = [shop];

        var config = SystemConfig.Create(mallId, Guid.NewGuid(), 0.5m, 3);
        config.Update(0.5m, 3, 10, Guid.NewGuid());
        stubs.SystemConfigs = [config];
        stubs.Users = [user];

        var handler = CreateHandler(stubs);

        var result = await handler.Handle(
            new ConfirmPointsRedemptionQr.Command(qrCode.QrId),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeTrue();
    }

    private static ConfirmPointsRedemptionQr.Handler CreateHandler(HandlerStubs stubs)
    {
        return new ConfirmPointsRedemptionQr.Handler(
            stubs.QrCodeRepo,
            stubs.RedeemTransactionRepo,
            stubs.UserRepo,
            stubs.AuditLogRepo,
            stubs.ShopRepo,
            stubs.SystemConfigReadRepo,
            stubs.DateTimeProvider,
            stubs.PointsRedemptionQrTokenProvider,
            stubs.ShopAdminContext);
    }

    private sealed class HandlerStubs
    {
        public List<QrCode> QrCodes { get; set; } = [];
        public List<RedeemTransaction> RedeemTransactions { get; set; } = [];
        public List<User> Users { get; set; } = [];
        public List<AuditLog> AuditLogs { get; set; } = [];
        public List<Shop> Shops { get; set; } = [];
        public List<SystemConfig> SystemConfigs { get; set; } = [];
        public PointsRedemptionQrTokenPayload? ValidatePayloadResult { get; set; }
        public ShopAdminContextStub ShopAdminContext { get; set; } = new ShopAdminContextStub(Guid.NewGuid(), Guid.NewGuid());

        public IRepository<QrCode> QrCodeRepo => new QrCodeRepositoryStub(QrCodes);
        public IRepository<RedeemTransaction> RedeemTransactionRepo => new RedeemTransactionRepositoryStub(RedeemTransactions);
        public IRepository<User> UserRepo => new UserRepositoryStub(Users);
        public IRepository<AuditLog> AuditLogRepo => new AuditLogRepositoryStub(AuditLogs);
        public IRepository<Shop> ShopRepo => new ShopRepositoryStub(Shops);
        public IReadOnlyRepository<SystemConfig> SystemConfigReadRepo => new SystemConfigReadOnlyRepositoryStub(SystemConfigs);
        public IDateTimeProvider DateTimeProvider => new DateTimeProviderStub(FixedUtcNow);
        public IPointsRedemptionQrTokenProvider PointsRedemptionQrTokenProvider =>
            new PointsRedemptionQrTokenProviderStub(ValidatePayloadResult);
    }

    private sealed class ShopAdminContextStub(Guid shopAdminId, Guid shopId) : IShopAdminContext
    {
        public Guid ShopAdminId { get; } = shopAdminId;
        public Guid ShopId { get; } = shopId;
    }

    private sealed class DateTimeProviderStub(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    private sealed class PointsRedemptionQrTokenProviderStub(PointsRedemptionQrTokenPayload? payload)
        : IPointsRedemptionQrTokenProvider
    {
        public string CreateToken(PointsRedemptionQrTokenPayload payload) => "token";
        public Task<PointsRedemptionQrTokenPayload?> ValidateAndGetPayloadAsync(string token) =>
            Task.FromResult(payload);
    }

    private sealed class QrCodeRepositoryStub(List<QrCode> items) : IRepository<QrCode>
    {
        public IQueryable<QrCode> Find(ISpecification<QrCode> spec) =>
            new TestAsyncEnumerable<QrCode>(items.Where(spec.Criteria?.Compile() ?? (_ => true)));
        public Task<int> CountAsync(ISpecification<QrCode> spec) => Task.FromResult(items.Count);
        public IQueryable<QrCode> GetAll() => new TestAsyncEnumerable<QrCode>(items);
        public int Count(ISpecification<QrCode> spec) => items.Count;
        public Task AddAsync(QrCode entity) { items.Add(entity); return Task.CompletedTask; }
        public void Add(QrCode entity) => items.Add(entity);
        public Task AddRangeAsync(IEnumerable<QrCode> entities) { items.AddRange(entities); return Task.CompletedTask; }
        public void AddRange(IEnumerable<QrCode> entities) => items.AddRange(entities);
        public Task DeleteAsync(QrCode entity) { items.Remove(entity); return Task.CompletedTask; }
        public void Delete(QrCode entity) => items.Remove(entity);
        public Task DeleteRangeAsync(IEnumerable<QrCode> entities) { foreach (var e in entities.ToList()) { items.Remove(e); } return Task.CompletedTask; }
        public void DeleteRange(IEnumerable<QrCode> entities) { foreach (var e in entities.ToList()) { items.Remove(e); } }
    }

    private sealed class UserRepositoryStub(List<User> items) : IRepository<User>
    {
        public IQueryable<User> Find(ISpecification<User> spec) =>
            new TestAsyncEnumerable<User>(items.Where(spec.Criteria?.Compile() ?? (_ => true)));
        public Task<int> CountAsync(ISpecification<User> spec) => Task.FromResult(items.Count);
        public IQueryable<User> GetAll() => new TestAsyncEnumerable<User>(items);
        public int Count(ISpecification<User> spec) => items.Count;
        public Task AddAsync(User entity) { items.Add(entity); return Task.CompletedTask; }
        public void Add(User entity) => items.Add(entity);
        public Task AddRangeAsync(IEnumerable<User> entities) { items.AddRange(entities); return Task.CompletedTask; }
        public void AddRange(IEnumerable<User> entities) => items.AddRange(entities);
        public Task DeleteAsync(User entity) { items.Remove(entity); return Task.CompletedTask; }
        public void Delete(User entity) => items.Remove(entity);
        public Task DeleteRangeAsync(IEnumerable<User> entities) { foreach (var e in entities.ToList()) { items.Remove(e); } return Task.CompletedTask; }
        public void DeleteRange(IEnumerable<User> entities) { foreach (var e in entities.ToList()) { items.Remove(e); } }
    }

    private sealed class ShopRepositoryStub(List<Shop> items) : IRepository<Shop>
    {
        public IQueryable<Shop> Find(ISpecification<Shop> spec) =>
            new TestAsyncEnumerable<Shop>(items.Where(spec.Criteria?.Compile() ?? (_ => true)));
        public Task<int> CountAsync(ISpecification<Shop> spec) => Task.FromResult(items.Count);
        public IQueryable<Shop> GetAll() => new TestAsyncEnumerable<Shop>(items);
        public int Count(ISpecification<Shop> spec) => items.Count;
        public Task AddAsync(Shop entity) { items.Add(entity); return Task.CompletedTask; }
        public void Add(Shop entity) => items.Add(entity);
        public Task AddRangeAsync(IEnumerable<Shop> entities) { items.AddRange(entities); return Task.CompletedTask; }
        public void AddRange(IEnumerable<Shop> entities) => items.AddRange(entities);
        public Task DeleteAsync(Shop entity) { items.Remove(entity); return Task.CompletedTask; }
        public void Delete(Shop entity) => items.Remove(entity);
        public Task DeleteRangeAsync(IEnumerable<Shop> entities) { foreach (var e in entities.ToList()) { items.Remove(e); } return Task.CompletedTask; }
        public void DeleteRange(IEnumerable<Shop> entities) { foreach (var e in entities.ToList()) { items.Remove(e); } }
    }

    private sealed class RedeemTransactionRepositoryStub(List<RedeemTransaction> items) : IRepository<RedeemTransaction>
    {
        public IQueryable<RedeemTransaction> Find(ISpecification<RedeemTransaction> spec) =>
            new TestAsyncEnumerable<RedeemTransaction>(items);
        public Task<int> CountAsync(ISpecification<RedeemTransaction> spec) => Task.FromResult(items.Count);
        public IQueryable<RedeemTransaction> GetAll() => new TestAsyncEnumerable<RedeemTransaction>(items);
        public int Count(ISpecification<RedeemTransaction> spec) => items.Count;
        public Task AddAsync(RedeemTransaction entity) { items.Add(entity); return Task.CompletedTask; }
        public void Add(RedeemTransaction entity) => items.Add(entity);
        public Task AddRangeAsync(IEnumerable<RedeemTransaction> entities) { items.AddRange(entities); return Task.CompletedTask; }
        public void AddRange(IEnumerable<RedeemTransaction> entities) => items.AddRange(entities);
        public Task DeleteAsync(RedeemTransaction entity) { items.Remove(entity); return Task.CompletedTask; }
        public void Delete(RedeemTransaction entity) => items.Remove(entity);
        public Task DeleteRangeAsync(IEnumerable<RedeemTransaction> entities) { foreach (var e in entities.ToList()) { items.Remove(e); } return Task.CompletedTask; }
        public void DeleteRange(IEnumerable<RedeemTransaction> entities) { foreach (var e in entities.ToList()) { items.Remove(e); } }
    }

    private sealed class AuditLogRepositoryStub(List<AuditLog> items) : IRepository<AuditLog>
    {
        public IQueryable<AuditLog> Find(ISpecification<AuditLog> spec) =>
            new TestAsyncEnumerable<AuditLog>(items);
        public Task<int> CountAsync(ISpecification<AuditLog> spec) => Task.FromResult(items.Count);
        public IQueryable<AuditLog> GetAll() => new TestAsyncEnumerable<AuditLog>(items);
        public int Count(ISpecification<AuditLog> spec) => items.Count;
        public Task AddAsync(AuditLog entity) { items.Add(entity); return Task.CompletedTask; }
        public void Add(AuditLog entity) => items.Add(entity);
        public Task AddRangeAsync(IEnumerable<AuditLog> entities) { items.AddRange(entities); return Task.CompletedTask; }
        public void AddRange(IEnumerable<AuditLog> entities) => items.AddRange(entities);
        public Task DeleteAsync(AuditLog entity) { items.Remove(entity); return Task.CompletedTask; }
        public void Delete(AuditLog entity) => items.Remove(entity);
        public Task DeleteRangeAsync(IEnumerable<AuditLog> entities) { foreach (var e in entities.ToList()) { items.Remove(e); } return Task.CompletedTask; }
        public void DeleteRange(IEnumerable<AuditLog> entities) { foreach (var e in entities.ToList()) { items.Remove(e); } }
    }

    private sealed class SystemConfigReadOnlyRepositoryStub(List<SystemConfig> items) : IReadOnlyRepository<SystemConfig>
    {
        public IQueryable<SystemConfig> Find(ISpecification<SystemConfig> spec) =>
            new TestAsyncEnumerable<SystemConfig>(items.Where(spec.Criteria?.Compile() ?? (_ => true)));
        public Task<int> CountAsync(ISpecification<SystemConfig> spec) => Task.FromResult(items.Count);
        public IQueryable<SystemConfig> GetAll() => new TestAsyncEnumerable<SystemConfig>(items);
        public int Count(ISpecification<SystemConfig> spec) => items.Count;
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
