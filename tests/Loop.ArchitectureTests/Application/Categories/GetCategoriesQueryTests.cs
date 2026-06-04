using System.Linq.Expressions;
using Loop.Application.Categories.Query;
using Loop.Application.Interfaces;
using Loop.Domain.Shops;
using Loop.Domain.Specifications;
using Loop.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore.Query;
using Shouldly;

namespace Loop.ArchitectureTests.Application.Categories;

public class GetCategoriesQueryTests
{
    [Fact]
    public async Task Handle_ShouldReturnCategoriesForMall()
    {
        var mallId = Guid.NewGuid();
        var categories = new[]
        {
            Category.Create(mallId, "Coffee", "/icons/coffee.png", "Coffee shops", 1),
            Category.Create(mallId, "Bakery", "/icons/bakery.png", "Bakery shops", 2)
        };

        var repository = new CategoryReadOnlyRepositoryStub(categories);
        var handler = new GetCategories.Handler(repository);

        var result = await handler.Handle(new GetCategories.Query(mallId), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Count.ShouldBe(2);
        result.Value[0].CategoryId.ShouldBe(categories[0].CategoryId);
        result.Value[0].CategoryName.ShouldBe("Coffee");
        result.Value[1].CategoryName.ShouldBe("Bakery");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenMallHasNoCategories()
    {
        var repository = new CategoryReadOnlyRepositoryStub([]);
        var handler = new GetCategories.Handler(repository);

        var result = await handler.Handle(new GetCategories.Query(Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    private sealed class CategoryReadOnlyRepositoryStub(IEnumerable<Category> categories) : IReadOnlyRepository<Category>
    {
        private readonly List<Category> _categories = categories.ToList();

        public IQueryable<Category> Find(ISpecification<Category> spec)
        {
            IQueryable<Category> query = _categories.AsQueryable();

            if (spec.Criteria is not null)
            {
                query = query.Where(spec.Criteria.Compile()).AsQueryable();
            }

            return new TestAsyncEnumerable<Category>(query);
        }

        public Task<int> CountAsync(ISpecification<Category> spec)
        {
            int count = spec.Criteria is null
                ? _categories.Count
                : _categories.Count(spec.Criteria.Compile());

            return Task.FromResult(count);
        }

        public IQueryable<Category> GetAll() => new TestAsyncEnumerable<Category>(_categories);

        public int Count(ISpecification<Category> spec) =>
            spec.Criteria is null
                ? _categories.Count
                : _categories.Count(spec.Criteria.Compile());
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