using Application.Interfaces;
using Infrastructure.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace SharedKernel.UnitOfWork;
public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private IDbContextTransaction? _transaction;
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;
    public UnitOfWork(DbContext context, IDomainEventsDispatcher domainEventsDispatcher)
    {
        _context = context;
        _domainEventsDispatcher= domainEventsDispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        await PublishDomainEventsAsync();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public void CreateTransaction()
    {
        _transaction = _context.Database.BeginTransaction();
    }

    public async Task CreateTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public void CommitTransaction()
    {
        try
        {
            _transaction?.Commit();
        }
        catch
        {
            Rollback();
            throw;
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public void Rollback()
    {
        try
        {
            _transaction?.Rollback();
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    public async Task RollbackAsync()
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task PublishDomainEventsAsync()
    {
        var domainEvents = _context.ChangeTracker
            .Entries<Loop.SharedKernel.AggregateRoot>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = entity.DomainEvents;

                entity.ClearDomainEvents();

                return domainEvents;
            })
            .ToList();

        await _domainEventsDispatcher.DispatchAsync(domainEvents);
    }
}
