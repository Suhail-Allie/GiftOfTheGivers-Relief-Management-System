using Microsoft.AspNetCore.Identity;

namespace GiftOfTheGivers.Web.Data;

public static class IdentitySeed
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        string[] roles =
        {
            "Employee",
            "Donor"
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(
                    new IdentityRole(roleName));

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        roleResult.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Could not create role '{roleName}': {errors}");
                }
            }
        }

        var employeeEmail =
            configuration["SeedUsers:EmployeeEmail"];

        var employeePassword =
            configuration["SeedUsers:EmployeePassword"];

        if (string.IsNullOrWhiteSpace(employeeEmail) ||
            string.IsNullOrWhiteSpace(employeePassword))
        {
            throw new InvalidOperationException(
                "Employee seed credentials were not configured.");
        }

        var employeeUser =
            await userManager.FindByEmailAsync(employeeEmail);

        if (employeeUser == null)
        {
            employeeUser = new IdentityUser
            {
                UserName = employeeEmail,
                Email = employeeEmail,
                EmailConfirmed = true
            };

            var createResult =
                await userManager.CreateAsync(
                    employeeUser,
                    employeePassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Could not create Employee account: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(
                employeeUser,
                "Employee"))
        {
            var roleResult =
                await userManager.AddToRoleAsync(
                    employeeUser,
                    "Employee");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Could not assign Employee role: {errors}");
            }
        }
    }
}