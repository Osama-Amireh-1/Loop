namespace Loop.Application.Abstractions.Storage;

public interface IImageFileStore
{
    Task<string> SaveAsync(Guid userId, string fileName, byte[] content, CancellationToken cancellationToken = default);
}
