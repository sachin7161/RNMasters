
using Microsoft.AspNetCore.Identity;
using RNMasterInfrastructure.Identity;

namespace RNMasterInfrastructure.Persistence.Seeder;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        RNMasterDbContext context,
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager)
    {
        // --------------------------------------------
        // Read roles from .env
        // --------------------------------------------

        var rolesValue =
            Environment.GetEnvironmentVariable("IDENTITY_ROLES");

        var roles = (rolesValue ?? "User")
            .Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries)
            .Select(role => role.Trim())
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // --------------------------------------------
        // Create missing roles
        // --------------------------------------------

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new AppRole
                    {
                        Name = role,
                        TenantId = 1
                    });

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to create role '{role}': {errors}");
                }
            }
        }

        // --------------------------------------------
        // Initial Super Admin
        // --------------------------------------------

        var adminEmail =
            Environment.GetEnvironmentVariable("ADMIN_EMAIL")
            ?? "admin@rnmaster.com";

        var existingAdmin =
            await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin == null)
        {
            // --------------------------------------------
            // Create initial Super Admin user
            // --------------------------------------------

            var adminPassword =
                Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "ADMIN_PASSWORD is not configured.");
            }

            var adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "RNMaster Super Admin",
                TenantId = 1,
                EmailConfirmed = true
            };

            var userResult = await userManager.CreateAsync(
                adminUser,
                adminPassword);

            if (!userResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    userResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to create initial Super Admin user: {errors}");
            }

            // --------------------------------------------
            // Assign Super Admin role
            // --------------------------------------------

            var superAdminRole = roles.FirstOrDefault(
                role => string.Equals(
                    role,
                    "SuperAdmin",
                    StringComparison.OrdinalIgnoreCase));

            if (superAdminRole != null)
            {
                var roleResult = await userManager.AddToRoleAsync(
                    adminUser,
                    superAdminRole);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to assign Super Admin role: {errors}");
                }
            }
            else
            {
                throw new InvalidOperationException(
                    "SuperAdmin role is missing from IDENTITY_ROLES.");
            }
        }
    }
}
