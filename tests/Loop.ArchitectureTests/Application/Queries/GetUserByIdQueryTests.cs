using System.Linq.Expressions;
using Loop.Application.Abstractions.Authentication;
using Loop.Application.Interfaces;
using Loop.Application.Users.Query;
using Loop.Domain.Common;
using Loop.Domain.Users;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Queries;

public class GetUserByIdQueryTests
{
    [Fact]
    public async Task Handle_ShouldReturnUnauthorized_WhenRequestedUserIsNotCurrentUser()
    {
        var repository = new UserReadOnlyRepositoryStub([]);
        IUserContext userContext = new TestUserContext(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        var handler = new GetUserById.GetUserByIdQueryHandler(repository, userContext);

        var query = new GetUserById.GetUserByIdQuery(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.Unauthorized");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        Guid userId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var repository = new UserReadOnlyRepositoryStub([]);
        IUserContext userContext = new TestUserContext(userId);

        var handler = new GetUserById.GetUserByIdQueryHandler(repository, userContext);

        var result = await handler.Handle(new GetUserById.GetUserByIdQuery(userId), CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.Code.ShouldBe("Users.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnUserResponse_WhenUserExists()
    {
        var user = User.Create(
            "John",
            "Doe",
            Phone.Create("123456789"),
            Email.Create("john.doe@example.com"),
            "hash",
            Gender.Male,
            Guid.NewGuid());

        var repository = new UserReadOnlyRepositoryStub([user]);
        IUserContext userContext = new TestUserContext(user.UserId);

        var handler = new GetUserById.GetUserByIdQueryHandler(repository, userContext);

        var result = await handler.Handle(new GetUserById.GetUserByIdQuery(user.UserId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(user.UserId);
        result.Value.FirstName.ShouldBe(user.FirstName);
        result.Value.LastName.ShouldBe(user.LastName);
        result.Value.Email.ShouldBe(user.Email.Value);
    }

    private sealed class TestUserContext(Guid userId) : IUserContext
    {
        public Guid UserId { get; } = userId;
    }

    private sealed class UserReadOnlyRepositoryStub(IEnumerable<User> users) : IReadOnlyRepository<User>
    {
        private readonly List<User> _users = users.ToList();

        public IQueryable<User> Find(ISpecification<User> spec)
        {
            IQueryable<User> query = _users.AsQueryable();

            if (spec.Criteria is not null)
            {
                query = query.Where(spec.Criteria.Compile()).AsQueryable();
            }

            return new TestAsyncEnumerable<User>(query);
        }

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
