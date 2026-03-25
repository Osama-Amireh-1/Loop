namespace Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    int SaveChanges();
    void CreateTransaction();
    Task CreateTransactionAsync();
    void CommitTransaction();
    Task CommitTransactionAsync();
    void Rollback();
    Task RollbackAsync();

}
