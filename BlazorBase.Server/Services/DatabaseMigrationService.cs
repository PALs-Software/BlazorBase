using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorBase.CRUD.Services;

public static class DatabaseMigrationService
{
    public static void MigrateDatabase<TDbContext>(IApplicationBuilder app) where TDbContext : DbContext
    {
        using var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        scope.ServiceProvider.GetRequiredService<TDbContext>().Database.Migrate();
    }
}
