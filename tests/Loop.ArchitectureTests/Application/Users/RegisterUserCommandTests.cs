using System.Linq.Expressions;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Users.Command;
using Loop.Domain.Common;
using Loop.Domain.Tiers;
using Loop.Domain.Users;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Users;

public class RegisterUserCommandTests
{
    [Fact]
    public async Task Handle_ShouldFail_WhenEmailIsInvalid()
    {
        var userRepo = new UserRepositoryStub([]);
        var tierReadRepo = new TierReadOnlyRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub("hash");
        var handler = new RegisterUser.Handler(userRepo, tierReadRepo, passwordHasher);

        var result = await handler.Handle(
            new RegisterUser.RegisterUserCommand("not-an-email", "John", "Doe", "0790000000", "Male", "password123"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.Email.Invalid");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenEmailNotUnique()
    {
        var email = Email.Create("user@test.com").Value;
        var existingUser = User.Create("Jane", "Doe", Phone.Create("0790000001").Value, email, "hash", Loop.Domain.Common.Gender.Male, Guid.NewGuid());
        var userRepo = new UserRepositoryStub([existingUser]);
        var tierReadRepo = new TierReadOnlyRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub("hash");
        var handler = new RegisterUser.Handler(userRepo, tierReadRepo, passwordHasher);

        var result = await handler.Handle(
            new RegisterUser.RegisterUserCommand("user@test.com", "John", "Doe", "0790000000", "Male", "password123"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.EmailNotUnique);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenPhoneIsInvalid()
    {
        var userRepo = new UserRepositoryStub([]);
        var tierReadRepo = new TierReadOnlyRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub("hash");
        var handler = new RegisterUser.Handler(userRepo, tierReadRepo, passwordHasher);

        var result = await handler.Handle(
            new RegisterUser.RegisterUserCommand("user@test.com", "John", "Doe", "ab", "Male", "password123"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.Phone.InvalidLength");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenGenderIsInvalid()
    {
        var userRepo = new UserRepositoryStub([]);
        var tierReadRepo = new TierReadOnlyRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub("hash");
        var handler = new RegisterUser.Handler(userRepo, tierReadRepo, passwordHasher);

        var result = await handler.Handle(
            new RegisterUser.RegisterUserCommand("user@test.com", "John", "Doe", "0790000000", "InvalidGender", "password123"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(UserErrors.InvalidGender);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDefaultTierNotFound()
    {
        var userRepo = new UserRepositoryStub([]);
        var tierReadRepo = new TierReadOnlyRepositoryStub([]);
        var passwordHasher = new PasswordHasherStub("hash");
        var handler = new RegisterUser.Handler(userRepo, tierReadRepo, passwordHasher);

        var result = await handler.Handle(
            new RegisterUser.RegisterUserCommand("user@test.com", "John", "Doe", "0790000000", "Male", "password123"),
            CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe(TierErrors.NotFound(1).Code);
    }

    [Fact]
    public async Task Handle_ShouldSucceed_WhenValidInput()
    {
        var userRepo = new UserRepositoryStub([]);
        var tier = Tier.Create("Bronze", 0, null, "icon.png", "#CD7F32");
        var tierReadRepo = new TierReadOnlyRepositoryStub([tier]);
        var passwordHasher = new PasswordHasherStub("hashed-password");
        var handler = new RegisterUser.Handler(userRepo, tierReadRepo, passwordHasher);

        var result = await handler.Handle(
            new RegisterUser.RegisterUserCommand("user@test.com", "John", "Doe", "0790000000", "Male", "password123"),
            CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBe(Guid.Empty);
    }

    private sealed class PasswordHasherStub(string hashResult) : IPasswordHasher
    {
        public string Hash(string password) => hashResult;
        public bool Verify(string password, string passwordHash) => true;
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

    private sealed class TierReadOnlyRepositoryStub(IEnumerable<Tier> items) : IReadOnlyRepository<Tier>
    {
        private readonly List<Tier> _items = items.ToList();
        public IQueryable<Tier> Find(ISpecification<Tier> spec) => new TestAsyncEnumerable<Tier>(_items);
        public Task<int> CountAsync(ISpecification<Tier> spec) => Task.FromResult(_items.Count);
        public IQueryable<Tier> GetAll() => new TestAsyncEnumerable<Tier>(_items);
        public int Count(ISpecification<Tier> spec) => _items.Count;
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
