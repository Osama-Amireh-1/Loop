using System.Linq.Expressions;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Shops.Command;
using Loop.Application.Users.Contract;
using Loop.Domain.Common;
using Loop.Domain.Shops;
using Loop.Domain.Users;
using Loop.SharedKernel;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Shops;

public class LoginShopAdminCommandTests
{
    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenEmailIsInvalid()
    {
        var shopAdminRepo = new ShopAdminRepositoryStub([]);
        var sessionRepo = new ShopAdminSessionRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub(true);
        var tokenProvider = new TokenProviderStub();
        var handler = new LoginShopAdmin.Handler(shopAdminRepo, sessionRepo, passwordHasher, tokenProvider);

        var result = await handler.Handle(
            new LoginShopAdmin.LoginShopAdminCommand("not-an-email", "password123"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.Email.Invalid");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenAdminDoesNotExist()
    {
        var shopAdminRepo = new ShopAdminRepositoryStub([]);
        var sessionRepo = new ShopAdminSessionRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub(true);
        var tokenProvider = new TokenProviderStub();
        var handler = new LoginShopAdmin.Handler(shopAdminRepo, sessionRepo, passwordHasher, tokenProvider);

        var result = await handler.Handle(
            new LoginShopAdmin.LoginShopAdminCommand("admin@shop.com", "password123"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Shops.AdminNotFoundByEmail");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenPasswordIsWrong()
    {
        var email = Email.Create("admin@shop.com").Value;
        var phone = Phone.Create("0790000000").Value;
        var shopAdmin = ShopAdmin.Create(Guid.NewGuid(), "Admin", email, phone, "hash", Loop.Domain.Shops.ShopAdminRole.Staff);
        var shopAdminRepo = new ShopAdminRepositoryStub([shopAdmin]);
        var sessionRepo = new ShopAdminSessionRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub(false);
        var tokenProvider = new TokenProviderStub();
        var handler = new LoginShopAdmin.Handler(shopAdminRepo, sessionRepo, passwordHasher, tokenProvider);

        var result = await handler.Handle(
            new LoginShopAdmin.LoginShopAdminCommand("admin@shop.com", "wrongpassword"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Shops.AdminNotFoundByEmail");
    }

    [Fact]
    public async Task Handle_ShouldReturnTokens_WhenLoginSucceeds()
    {
        var email = Email.Create("admin@shop.com").Value;
        var phone = Phone.Create("0790000000").Value;
        var shopAdmin = ShopAdmin.Create(Guid.NewGuid(), "Admin", email, phone, "hash", Loop.Domain.Shops.ShopAdminRole.Staff);
        var shopAdminRepo = new ShopAdminRepositoryStub([shopAdmin]);
        var sessionRepo = new ShopAdminSessionRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub(true);
        var tokenProvider = new TokenProviderStub();
        var handler = new LoginShopAdmin.Handler(shopAdminRepo, sessionRepo, passwordHasher, tokenProvider);

        var result = await handler.Handle(
            new LoginShopAdmin.LoginShopAdminCommand("admin@shop.com", "correctpassword"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe("test-access-token");
        result.Value.RefreshToken.ShouldBe("test-refresh-token");
    }

    private sealed class PasswordHasherStub(bool verifyResult) : IPasswordHasher
    {
        public string Hash(string password) => "hashed";
        public bool Verify(string password, string passwordHash) => verifyResult;
    }

    private sealed class TokenProviderStub : ITokenProvider
    {
        public string CreateAccessToken(Loop.Domain.Users.User user) => "test-access-token";
        public string CreateAccessToken(ShopAdmin shopAdmin) => "test-access-token";
        public (string RefreshToken, DateTime ExpiresAtUtc) CreateRefreshToken() =>
            ("test-refresh-token", DateTime.UtcNow.AddDays(7));
    }

    private sealed class ShopAdminRepositoryStub(IEnumerable<ShopAdmin> admins) : IRepository<ShopAdmin>
    {
        private readonly List<ShopAdmin> _admins = admins.ToList();

        public IQueryable<ShopAdmin> Find(ISpecification<ShopAdmin> spec) =>
            new TestAsyncEnumerable<ShopAdmin>(_admins);

        public Task<int> CountAsync(ISpecification<ShopAdmin> spec) =>
            Task.FromResult(_admins.Count);

        public IQueryable<ShopAdmin> GetAll() => new TestAsyncEnumerable<ShopAdmin>(_admins);

        public int Count(ISpecification<ShopAdmin> spec) => _admins.Count;

        public Task AddAsync(ShopAdmin entity)
        {
            _admins.Add(entity);
            return Task.CompletedTask;
        }

        public void Add(ShopAdmin entity) => _admins.Add(entity);

        public Task AddRangeAsync(IEnumerable<ShopAdmin> entities)
        {
            _admins.AddRange(entities);
            return Task.CompletedTask;
        }

        public void AddRange(IEnumerable<ShopAdmin> entities) => _admins.AddRange(entities);

        public Task DeleteAsync(ShopAdmin entity)
        {
            _admins.Remove(entity);
            return Task.CompletedTask;
        }

        public void Delete(ShopAdmin entity) => _admins.Remove(entity);

        public Task DeleteRangeAsync(IEnumerable<ShopAdmin> entities)
        {
            foreach (var e in entities.ToList()) { _admins.Remove(e); }
            return Task.CompletedTask;
        }

        public void DeleteRange(IEnumerable<ShopAdmin> entities)
        {
            foreach (var e in entities.ToList()) { _admins.Remove(e); }
        }
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
