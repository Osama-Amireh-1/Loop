using System.Linq.Expressions;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Users.Command;
using Loop.Domain.Users;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Users;

public class LogoutUserCommandTests
{
    [Fact]
    public async Task Handle_ShouldFail_WhenUserNotFound()
    {
        var userRepo = new UserRepositoryStub([]);
        var sessionRepo = new UserSessionRepositoryStub([]);
        var context = new UserContextStub(Guid.NewGuid());
        var handler = new LogoutUser.Handler(userRepo, sessionRepo, context);

        var result = await handler.Handle(
            new LogoutUser.LogoutUserCommand(),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenUserExists()
    {
        var email = Loop.Domain.Common.Email.Create("user@test.com").Value;
        var phone = Loop.Domain.Common.Phone.Create("0790000000").Value;
        var user = User.Create("John", "Doe", phone, email, "hash", Loop.Domain.Common.Gender.Male, Guid.NewGuid());

        var session1 = UserSession.Create(user.UserId, "hash1", DateTime.UtcNow.AddDays(1));
        var session2 = UserSession.Create(user.UserId, "hash2", DateTime.UtcNow.AddDays(1));

        var userRepo = new UserRepositoryStub([user]);
        var sessionRepo = new UserSessionRepositoryStub([session1, session2]);
        var context = new UserContextStub(user.UserId);
        var handler = new LogoutUser.Handler(userRepo, sessionRepo, context);

        var result = await handler.Handle(
            new LogoutUser.LogoutUserCommand(),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        sessionRepo._sessions.Count.ShouldBe(0);
    }

    private sealed class UserContextStub(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }

    private sealed class UserRepositoryStub(IEnumerable<User> items) : IRepository<User>
    {
        public List<User> _items = items.ToList();
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
        public List<UserSession> _sessions = items.ToList();
        public IQueryable<UserSession> Find(ISpecification<UserSession> spec) => new TestAsyncEnumerable<UserSession>(_sessions);
        public Task<int> CountAsync(ISpecification<UserSession> spec) => Task.FromResult(_sessions.Count);
        public IQueryable<UserSession> GetAll() => new TestAsyncEnumerable<UserSession>(_sessions);
        public int Count(ISpecification<UserSession> spec) => _sessions.Count;
        public Task AddAsync(UserSession entity) { _sessions.Add(entity); return Task.CompletedTask; }
        public void Add(UserSession entity) => _sessions.Add(entity);
        public Task AddRangeAsync(IEnumerable<UserSession> entities) { _sessions.AddRange(entities); return Task.CompletedTask; }
        public void AddRange(IEnumerable<UserSession> entities) => _sessions.AddRange(entities);
        public Task DeleteAsync(UserSession entity) { _sessions.Remove(entity); return Task.CompletedTask; }
        public void Delete(UserSession entity) => _sessions.Remove(entity);
        public Task DeleteRangeAsync(IEnumerable<UserSession> entities) { foreach (var e in entities.ToList()) { _sessions.Remove(e); } return Task.CompletedTask; }
        public void DeleteRange(IEnumerable<UserSession> entities) { foreach (var e in entities.ToList()) { _sessions.Remove(e); } }
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
