using GiftOfTheGivers.Web.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Data.Relief;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(ReliefManagementContext context)
    {
        // Seed currencies needed for donation records.
        var currencies = new[]
        {
            new Currency
            {
                CurrencyCode = "ZAR",
                CurrencyName = "South African Rand",
                IsActive = true
            },
            new Currency
            {
                CurrencyCode = "USD",
                CurrencyName = "US Dollar",
                IsActive = true
            },
            new Currency
            {
                CurrencyCode = "EUR",
                CurrencyName = "Euro",
                IsActive = true
            }
        };

        foreach (var currency in currencies)
        {
            if (!await context.Currencies.AnyAsync(
                    c => c.CurrencyCode == currency.CurrencyCode))
            {
                context.Currencies.Add(currency);
            }
        }

        await context.SaveChangesAsync();

        // Create a domain user for local employee workflow testing.
        const string employeeEmail = "employee@giftofthegivers.local";

        var user = await context.Users
            .SingleOrDefaultAsync(u => u.Email == employeeEmail);

        if (user is null)
        {
            user = new User
            {
                Email = employeeEmail,
                PasswordHash = "Development-only placeholder",
                UserRole = "Employee",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Link the employee to the domain user.
        var employee = await context.Employees
            .SingleOrDefaultAsync(e => e.UserId == user.UserId);

        if (employee is null)
        {
            employee = new Employee
            {
                UserId = user.UserId,
                FullName = "Development Test Employee",
                JobTitle = "Relief Operations Employee",
                CreatedAt = DateTime.UtcNow
            };

            context.Employees.Add(employee);
            await context.SaveChangesAsync();
        }

        // Create an active project for local donation testing.
        const string projectName = "Local Test Relief Project";

        var project = await context.ReliefProjects
            .SingleOrDefaultAsync(p => p.ProjectName == projectName);

        if (project is null)
        {
            project = new ReliefProject
            {
                ProjectName = projectName,
                Description =
                    "Development-only project for testing donation workflows.",
                Location = "Cape Town",
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                ProjectStatus = "Active",
                CreatedByEmployeeId = employee.EmployeeId,
                CreatedAt = DateTime.UtcNow
            };

            context.ReliefProjects.Add(project);
            await context.SaveChangesAsync();
        }
    }
}