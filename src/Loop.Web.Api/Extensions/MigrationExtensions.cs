using Loop.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Loop.Web.Api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using LoopContext dbContext =
            scope.ServiceProvider.GetRequiredService<LoopContext>();

        dbContext.Database.Migrate();
    }
}

