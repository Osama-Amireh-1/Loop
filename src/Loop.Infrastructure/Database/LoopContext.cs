using Loop.Domain.Users;
using Loop.Infrastructure.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Loop.SharedKernel;

namespace Loop.Infrastructure.Database;

public sealed class LoopContext(
    DbContextOptions<LoopContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LoopContext).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);
    }

}


