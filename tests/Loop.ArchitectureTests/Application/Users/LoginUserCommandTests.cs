using System.Linq.Expressions;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Users.Command;
using Loop.Application.Users.Contract;
using Loop.Domain.Common;
using Loop.Domain.Shops;
using Loop.Domain.Users;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Users;

public class LoginUserCommandTests
{
    [Fact]
    public async Task Handle_ShouldFail_WhenEmailIsInvalid()
    {
        var userRepo = new UserRepositoryStub([]);
        var sessionRepo = new UserSessionRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub(true);
        var tokenProvider = new TokenProviderStub();
        var handler = new LoginUser.Handler(userRepo, sessionRepo, passwordHasher, tokenProvider);

        var result = await handler.Handle(
            new LoginUser.LoginUserCommand("not-an-email", "password123"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.Email.Invalid");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        var userRepo = new UserRepositoryStub([]);
        var sessionRepo = new UserSessionRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub(true);
        var tokenProvider = new TokenProviderStub();
        var handler = new LoginUser.Handler(userRepo, sessionRepo, passwordHasher, tokenProvider);

        var result = await handler.Handle(
            new LoginUser.LoginUserCommand("user@test.com", "password123"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPasswordIsWrong()
    {
        var email = Email.Create("user@test.com").Value;
        var phone = Phone.Create("0790000000").Value;
        var user = User.Create("John", "Doe", phone, email, "hash", Gender.Male, Guid.NewGuid());
        var userRepo = new UserRepositoryStub([user]);
        var sessionRepo = new UserSessionRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub(false);
        var tokenProvider = new TokenProviderStub();
        var handler = new LoginUser.Handler(userRepo, sessionRepo, passwordHasher, tokenProvider);

        var result = await handler.Handle(
            new LoginUser.LoginUserCommand("user@test.com", "wrong"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenCredentialsAreCorrect()
    {
        var email = Email.Create("user@test.com").Value;
        var phone = Phone.Create("0790000000").Value;
        var user = User.Create("John", "Doe", phone, email, "hash", Gender.Male, Guid.NewGuid());
        var userRepo = new UserRepositoryStub([user]);
        var sessionRepo = new UserSessionRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub(true);
        var tokenProvider = new TokenProviderStub();
        var handler = new LoginUser.Handler(userRepo, sessionRepo, passwordHasher, tokenProvider);

        var result = await handler.Handle(
            new LoginUser.LoginUserCommand("user@test.com", "correct"),
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
        public string CreateAccessToken(User user) => "test-access-token";
        public string CreateAccessToken(ShopAdmin shopAdmin) => "test-access-token";
        public (string RefreshToken, DateTime ExpiresAtUtc) CreateRefreshToken() =>
            ("test-refresh-token", DateTime.UtcNow.AddDays(7));
    }

    private sealed class UserRepositoryStub(IEnumerable<User> items) : IRepository<User>
    {
        private readonly List<User> _items = items.ToList();
        public IQueryable<User> Find(ISpecification<User> spec) => new TestAsyncEnumerable<User>(_items);
        public Task<int> CountAsync(ISpecification<User> spec) => Task.FromResult(_items.Count);
        public IQueryable<User> GetAll() => new TestAsyncEnumerable<User>(_items);
        public int Count(ISpecification<User> spec) => _items.Count;
        public Task AddAsync(User entity) { _items.Add(entity); return Task.CompletedTask; }
        public void Add(User entity) => _items.Add(entity);
        public Task AddRangeAsync(IEnumerable<User> entities) { _items.AddRange(entities); return Task.CompletedTask; }
        public void AddRange(IEnumerable<User> entities) => _items.AddRange(entities);
        public Task DeleteAsync(User entity) { _items.Remove(entity); return Task.CompletedTask; }
        public void Delete(User entity) => _items.Remove(entity);
        public Task DeleteRangeAsync(IEnumerable<User> entities) { foreach (var e in entities.ToList()) { _items.Remove(e); } return Task.CompletedTask; }
        public void DeleteRange(IEnumerable<User> entities) { foreach (var e in entities.ToList()) { _items.Remove(e); } }
    }

    private sealed class UserSessionRepositoryStub(IEnumerable<UserSession> items) : IRepository<UserSession>
    {
        private readonly List<UserSession> _items = items.ToList();
        public IQueryable<UserSession> Find(ISpecification<UserSession> spec) => new TestAsyncEnumerable<UserSession>(_items);
        public Task<int> CountAsync(ISpecification<UserSession> spec) => Task.FromResult(_items.Count);
        public IQueryable<UserSession> GetAll() => new TestAsyncEnumerable<UserSession>(_items);
        public int Count(ISpecification<UserSession> spec) => _items.Count;
        public Task AddAsync(UserSession entity) { _items.Add(entity); return Task.CompletedTask; }
        public void Add(UserSession entity) => _items.Add(entity);
        public Task AddRangeAsync(IEnumerable<UserSession> entities) { _items.AddRange(entities); return Task.CompletedTask; }
        public void AddRange(IEnumerable<UserSession> entities) => _items.AddRange(entities);
        public Task DeleteAsync(UserSession entity) { _items.Remove(entity); return Task.CompletedTask; }
        public void Delete(UserSession entity) => _items.Remove(entity);
        public Task DeleteRangeAsync(IEnumerable<UserSession> entities) { foreach (var e in entities.ToList()) { _items.Remove(e); } return Task.CompletedTask; }
        public void DeleteRange(IEnumerable<UserSession> entities) { foreach (var e in entities.ToList()) { _items.Remove(e); } }
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
