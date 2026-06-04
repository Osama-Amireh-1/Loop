using System.Linq.Expressions;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Users.Query;
using Loop.Domain.Common;
using Loop.Domain.Configuration;
using Loop.Domain.Users;
using Loop.Domain.Specifications;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Users;

public class UserQueryTests
{
    [Fact]
    public async Task GetUserByEmail_ShouldReturnValidationError_WhenEmailIsInvalid()
    {
        var repository = new UserReadOnlyRepositoryStub([]);
        IUserContext userContext = new TestUserContext(Guid.NewGuid());
        var handler = new GetUserByEmail.Handler(repository, userContext);

        var result = await handler.Handle(new GetUserByEmail.Query("not-an-email"), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Common.Email.Invalid");
    }

    [Fact]
    public async Task GetUserByEmail_ShouldReturnUnauthorized_WhenQueryUserIsNotCurrentUser()
    {
        var user = CreateUser();
        var repository = new UserReadOnlyRepositoryStub([user]);
        IUserContext userContext = new TestUserContext(Guid.NewGuid());
        var handler = new GetUserByEmail.Handler(repository, userContext);

        var result = await handler.Handle(new GetUserByEmail.Query(user.Email.Value), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.Unauthorized");
    }

    [Fact]
    public async Task GetUserByEmail_ShouldReturnUserResponse_WhenUserExistsAndMatchesCurrentUser()
    {
        var user = CreateUser();
        var repository = new UserReadOnlyRepositoryStub([user]);
        IUserContext userContext = new TestUserContext(user.UserId);
        var handler = new GetUserByEmail.Handler(repository, userContext);

        var result = await handler.Handle(new GetUserByEmail.Query(user.Email.Value), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(user.UserId);
        result.Value.Email.ShouldBe(user.Email.Value);
        result.Value.FirstName.ShouldBe(user.FirstName);
        result.Value.LastName.ShouldBe(user.LastName);
        result.Value.Phone.ShouldBe(user.Phone.Value);
        result.Value.Gender.ShouldBe(user.Gender.ToString());
    }

    [Fact]
    public async Task GetUserPointsBalance_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var userRepository = new UserReadOnlyRepositoryStub([]);
        var configRepository = new SystemConfigReadOnlyRepositoryStub([CreateConfig()]);
        IUserContext userContext = new TestUserContext(Guid.NewGuid());

        var handler = new GetUserPointsBalance.Handler(userRepository, configRepository, userContext);

        var result = await handler.Handle(new GetUserPointsBalance.Query(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.NotFound");
    }

    [Fact]
    public async Task GetUserPointsBalance_ShouldReturnNotFound_WhenSystemConfigDoesNotExist()
    {
        var user = CreateUser();
        var userRepository = new UserReadOnlyRepositoryStub([user]);
        var configRepository = new SystemConfigReadOnlyRepositoryStub([]);
        IUserContext userContext = new TestUserContext(user.UserId);

        var handler = new GetUserPointsBalance.Handler(userRepository, configRepository, userContext);

        var result = await handler.Handle(new GetUserPointsBalance.Query(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Configuration.NotFound");
    }

    [Fact]
    public async Task GetUserPointsBalance_ShouldReturnCalculatedBalance_WhenDataExists()
    {
        var user = CreateUser();
        _ = user.CreditPoints(12);
        var config = CreateConfig(pointsToCurrencyRatio: 0.5m, minThreshold: 20);

        var userRepository = new UserReadOnlyRepositoryStub([user]);
        var configRepository = new SystemConfigReadOnlyRepositoryStub([config]);
        IUserContext userContext = new TestUserContext(user.UserId);

        var handler = new GetUserPointsBalance.Handler(userRepository, configRepository, userContext);

        var result = await handler.Handle(new GetUserPointsBalance.Query(config.MallId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalPoints.ShouldBe(12);
        result.Value.PointsToCurrencyRatio.ShouldBe(0.5m);
        result.Value.MinRedemptionThreshold.ShouldBe(20);
        result.Value.EvaluatedValue.ShouldBe(6m);
    }

    private static User CreateUser()
    {
        var phone = Phone.Create("1234567").Value;
        var email = Email.Create("john.doe@example.com").Value;

        return User.Create(
            "John",
            "Doe",
            phone,
            email,
            "hash",
            Gender.Male,
            Guid.NewGuid());
    }

    private static SystemConfig CreateConfig(
        decimal pointsToCurrencyRatio = 2m,
        decimal earnRate = 3m,
        int minThreshold = 10)
    {
        var config = SystemConfig.Create(Guid.NewGuid(), Guid.NewGuid(), pointsToCurrencyRatio, earnRate);
        _ = config.Update(pointsToCurrencyRatio, earnRate, minThreshold, Guid.NewGuid());
        return config;
    }

    private sealed class TestUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }

    private sealed class UserReadOnlyRepositoryStub(IEnumerable<User> users) : IReadOnlyRepository<User>
    {
        private readonly List<User> _users = users.ToList();

        public IQueryable<User> Find(ISpecification<User> spec) => new TestAsyncEnumerable<User>(_users);

        public Task<int> CountAsync(ISpecification<User> spec)
        {
            int count = spec.Criteria is null
                ? _users.Count
                : _users.Count(spec.Criteria.Compile());

            return Task.FromResult(count);
        }

        public IQueryable<User> GetAll() => new TestAsyncEnumerable<User>(_users);

        public int Count(ISpecification<User> spec) =>
            spec.Criteria is null
                ? _users.Count
                : _users.Count(spec.Criteria.Compile());
    }

    private sealed class SystemConfigReadOnlyRepositoryStub(IEnumerable<SystemConfig> configs) : IReadOnlyRepository<SystemConfig>
    {
        private readonly List<SystemConfig> _configs = configs.ToList();

        public IQueryable<SystemConfig> Find(ISpecification<SystemConfig> spec)
        {
            IQueryable<SystemConfig> query = _configs.AsQueryable();

            if (spec.Criteria is not null)
            {
                query = query.Where(spec.Criteria.Compile()).AsQueryable();
            }

            return new TestAsyncEnumerable<SystemConfig>(query);
        }

        public Task<int> CountAsync(ISpecification<SystemConfig> spec)
        {
            int count = spec.Criteria is null
                ? _configs.Count
                : _configs.Count(spec.Criteria.Compile());

            return Task.FromResult(count);
        }

        public IQueryable<SystemConfig> GetAll() => new TestAsyncEnumerable<SystemConfig>(_configs);

        public int Count(ISpecification<SystemConfig> spec) =>
            spec.Criteria is null
                ? _configs.Count
                : _configs.Count(spec.Criteria.Compile());
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