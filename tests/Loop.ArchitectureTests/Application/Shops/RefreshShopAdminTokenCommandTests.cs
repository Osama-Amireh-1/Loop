using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Shops.Command;
using Loop.Application.Users.Contract;
using Loop.Domain.Shops;
using Loop.Domain.Users;
using Loop.SharedKernel;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Shops;

public class RefreshShopAdminTokenCommandTests
{
    private static string HashToken(string token)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidToken_WhenSessionNotFound()
    {
        var sessionRepo = new ShopAdminSessionRepositoryStub([]);
        var adminRepo = new ShopAdminReadOnlyRepositoryStub([]);
        var tokenProvider = new TokenProviderStub();
        var handler = new RefreshShopAdminToken.Handler(sessionRepo, adminRepo, tokenProvider);

        var result = await handler.Handle(
            new RefreshShopAdminToken.RefreshShopAdminTokenCommand("invalid-token"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Shops.InvalidRefreshToken");
    }

    [Fact]
    public async Task Handle_ShouldReturnTokenExpired_WhenSessionIsExpired()
    {
        var shopAdminId = Guid.NewGuid();
        var session = ShopAdminSession.Create(shopAdminId, HashToken("expired-token"), DateTime.UtcNow.AddDays(1));
        typeof(ShopAdminSession)
            .GetProperty(nameof(ShopAdminSession.ExpiresAtUtc), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
            .SetValue(session, DateTime.UtcNow.AddDays(-1));

        var sessionRepo = new ShopAdminSessionRepositoryStub([session]);
        var adminRepo = new ShopAdminReadOnlyRepositoryStub([]);
        var tokenProvider = new TokenProviderStub();
        var handler = new RefreshShopAdminToken.Handler(sessionRepo, adminRepo, tokenProvider);

        var result = await handler.Handle(
            new RefreshShopAdminToken.RefreshShopAdminTokenCommand("expired-token"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Shops.RefreshTokenExpired");
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidToken_WhenShopAdminNotFound()
    {
        var shopAdminId = Guid.NewGuid();
        var session = ShopAdminSession.Create(shopAdminId, HashToken("orphan-token"), DateTime.UtcNow.AddDays(1));

        var sessionRepo = new ShopAdminSessionRepositoryStub([session]);
        var adminRepo = new ShopAdminReadOnlyRepositoryStub([]);
        var tokenProvider = new TokenProviderStub();
        var handler = new RefreshShopAdminToken.Handler(sessionRepo, adminRepo, tokenProvider);

        var result = await handler.Handle(
            new RefreshShopAdminToken.RefreshShopAdminTokenCommand("orphan-token"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Shops.InvalidRefreshToken");
    }

    [Fact]
    public async Task Handle_ShouldReturnNewTokens_WhenRefreshSucceeds()
    {
        var email = Loop.Domain.Common.Email.Create("admin@shop.com").Value;
        var phone = Loop.Domain.Common.Phone.Create("0790000000").Value;
        var shopAdmin = ShopAdmin.Create(Guid.NewGuid(), "Admin", email, phone, "hash", Loop.Domain.Shops.ShopAdminRole.Staff);
        var session = ShopAdminSession.Create(shopAdmin.ShopAdminId, HashToken("valid-token"), DateTime.UtcNow.AddDays(1));

        var sessionRepo = new ShopAdminSessionRepositoryStub([session]);
        var adminRepo = new ShopAdminReadOnlyRepositoryStub([shopAdmin]);
        var tokenProvider = new TokenProviderStub();
        var handler = new RefreshShopAdminToken.Handler(sessionRepo, adminRepo, tokenProvider);

        var result = await handler.Handle(
            new RefreshShopAdminToken.RefreshShopAdminTokenCommand("valid-token"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe("test-access-token");
        result.Value.RefreshToken.ShouldBe("test-refresh-token");
    }

    private sealed class TokenProviderStub : ITokenProvider
    {
        public string CreateAccessToken(Loop.Domain.Users.User user) => "test-access-token";
        public string CreateAccessToken(ShopAdmin shopAdmin) => "test-access-token";
        public (string RefreshToken, DateTime ExpiresAtUtc) CreateRefreshToken() =>
            ("test-refresh-token", DateTime.UtcNow.AddDays(7));
    }

    private sealed class ShopAdminSessionRepositoryStub(IEnumerable<ShopAdminSession> sessions) : IRepository<ShopAdminSession>
    {
        private readonly List<ShopAdminSession> _sessions = sessions.ToList();

        public IQueryable<ShopAdminSession> Find(ISpecification<ShopAdminSession> spec) =>
            new TestAsyncEnumerable<ShopAdminSession>(_sessions);

        public Task<int> CountAsync(ISpecification<ShopAdminSession> spec) =>
            Task.FromResult(_sessions.Count);

        public IQueryable<ShopAdminSession> GetAll() => new TestAsyncEnumerable<ShopAdminSession>(_sessions);
        public int Count(ISpecification<ShopAdminSession> spec) => _sessions.Count;

        public Task AddAsync(ShopAdminSession entity)
        {
            _sessions.Add(entity);
            return Task.CompletedTask;
        }

        public void Add(ShopAdminSession entity) => _sessions.Add(entity);

        public Task AddRangeAsync(IEnumerable<ShopAdminSession> entities)
        {
            _sessions.AddRange(entities);
            return Task.CompletedTask;
        }

        public void AddRange(IEnumerable<ShopAdminSession> entities) => _sessions.AddRange(entities);

        public Task DeleteAsync(ShopAdminSession entity)
        {
            _sessions.Remove(entity);
            return Task.CompletedTask;
        }

        public void Delete(ShopAdminSession entity) => _sessions.Remove(entity);

        public Task DeleteRangeAsync(IEnumerable<ShopAdminSession> entities)
        {
            foreach (var e in entities.ToList()) { _sessions.Remove(e); }
            return Task.CompletedTask;
        }

        public void DeleteRange(IEnumerable<ShopAdminSession> entities)
        {
            foreach (var e in entities.ToList()) { _sessions.Remove(e); }
        }
    }

    private sealed class ShopAdminReadOnlyRepositoryStub(IEnumerable<ShopAdmin> admins) : IReadOnlyRepository<ShopAdmin>
    {
        private readonly List<ShopAdmin> _admins = admins.ToList();

        public IQueryable<ShopAdmin> Find(ISpecification<ShopAdmin> spec) =>
            new TestAsyncEnumerable<ShopAdmin>(_admins);

        public Task<int> CountAsync(ISpecification<ShopAdmin> spec) =>
            Task.FromResult(_admins.Count);

        public IQueryable<ShopAdmin> GetAll() => new TestAsyncEnumerable<ShopAdmin>(_admins);
        public int Count(ISpecification<ShopAdmin> spec) => _admins.Count;
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
