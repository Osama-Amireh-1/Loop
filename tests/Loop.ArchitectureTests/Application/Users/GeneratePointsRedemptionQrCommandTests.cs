using System.Linq.Expressions;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Users.Command;
using Loop.Application.Users.Contract;
using Loop.Domain.Common;
using Loop.Domain.QRCode;
using Loop.Domain.Transactions;
using Loop.Domain.Users;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Loop.SharedKernel;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Users;

public class GeneratePointsRedemptionQrCommandTests
{
    private static readonly DateTime FixedNow = new(2026, 6, 4, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Handle_ShouldFail_WhenPointsToRedeemIsZeroOrNegative()
    {
        var qrCodeRepo = new QrCodeRepositoryStub([]);
        var userReadRepo = new UserReadOnlyRepositoryStub([]);
        var context = new UserContextStub(Guid.NewGuid());
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new PointsRedemptionQrTokenProviderStub();
        var handler = new GeneratePointsRedemptionQr.Handler(
            qrCodeRepo, userReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new GeneratePointsRedemptionQr.Command(0),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TransactionErrors.InvalidRedemptionPoints);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        var qrCodeRepo = new QrCodeRepositoryStub([]);
        var userReadRepo = new UserReadOnlyRepositoryStub([]);
        var context = new UserContextStub(Guid.NewGuid());
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new PointsRedemptionQrTokenProviderStub();
        var handler = new GeneratePointsRedemptionQr.Handler(
            qrCodeRepo, userReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new GeneratePointsRedemptionQr.Command(100),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenInsufficientPoints()
    {
        var email = Email.Create("test@test.com").Value;
        var phone = Phone.Create("0790000000").Value;
        var user = User.Create("John", "Doe", phone, email, "hash", Gender.Male, Guid.NewGuid());

        var qrCodeRepo = new QrCodeRepositoryStub([]);
        var userReadRepo = new UserReadOnlyRepositoryStub([user]);
        var context = new UserContextStub(user.UserId);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new PointsRedemptionQrTokenProviderStub();
        var handler = new GeneratePointsRedemptionQr.Handler(
            qrCodeRepo, userReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new GeneratePointsRedemptionQr.Command(100),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(TransactionErrors.InsufficientPoints);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenSufficientPoints()
    {
        var email = Email.Create("test@test.com").Value;
        var phone = Phone.Create("0790000000").Value;
        var user = User.Create("John", "Doe", phone, email, "hash", Gender.Male, Guid.NewGuid());
        user.CreditPoints(200);

        var qrCodeRepo = new QrCodeRepositoryStub([]);
        var userReadRepo = new UserReadOnlyRepositoryStub([user]);
        var context = new UserContextStub(user.UserId);
        var dateTimeProvider = new DateTimeProviderStub(FixedNow);
        var tokenProvider = new PointsRedemptionQrTokenProviderStub();
        var handler = new GeneratePointsRedemptionQr.Handler(
            qrCodeRepo, userReadRepo, context, dateTimeProvider, tokenProvider);

        var result = await handler.Handle(
            new GeneratePointsRedemptionQr.Command(100),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.QrId.ShouldNotBe(Guid.Empty);
        result.Value.ExpiresAtUtc.ShouldBe(FixedNow.AddMinutes(2));
    }

    private sealed class UserContextStub(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }

    private sealed class DateTimeProviderStub(DateTime now) : IDateTimeProvider
    {
        public DateTime UtcNow => now;
    }

    private sealed class PointsRedemptionQrTokenProviderStub : IPointsRedemptionQrTokenProvider
    {
        public string CreateToken(PointsRedemptionQrTokenPayload payload) => "test-qr-token";

        public Task<PointsRedemptionQrTokenPayload?> ValidateAndGetPayloadAsync(string token) =>
            Task.FromResult<PointsRedemptionQrTokenPayload?>(null);
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

    private sealed class UserReadOnlyRepositoryStub(IEnumerable<User> items) : IReadOnlyRepository<User>
    {
        private readonly List<User> _items = items.ToList();
        public IQueryable<User> Find(ISpecification<User> spec) => new TestAsyncEnumerable<User>(_items);
        public Task<int> CountAsync(ISpecification<User> spec) => Task.FromResult(_items.Count);
        public IQueryable<User> GetAll() => new TestAsyncEnumerable<User>(_items);
        public int Count(ISpecification<User> spec) => _items.Count;
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
