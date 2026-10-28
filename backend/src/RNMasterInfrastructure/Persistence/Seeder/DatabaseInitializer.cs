
using RNMasterInfrastructure.Identity;
using RNMasterInfrastructure.Persistence.Seeder;
using RNMasterInfrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RNMasterInfrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<RNMasterDbContext>();

        var pendingMigrations =
     await dbContext.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            await dbContext.Database.MigrateAsync();
        }
        else
        {
            Console.WriteLine(
                "Database is up to date. Skipping migrations.");
        }

        // Execute database SQL scripts
        var databaseScriptService =
            scope.ServiceProvider
                .GetRequiredService<DatabaseScriptService>();

        await databaseScriptService.ExecuteScriptsAsync();

        // Seed roles and initial Super Admin
        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<AppUser>>();

        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<RoleManager<AppRole>>();

        await DatabaseSeeder.SeedAsync(
            dbContext,
            userManager,
            roleManager);
    }
}
