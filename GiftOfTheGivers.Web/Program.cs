using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Data.Relief;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();
var isDemo = builder.Environment.IsEnvironment("Demo");

var azureSqlConnection =
    builder.Configuration.GetConnectionString("AzureSqlConnection");

var localIdentityConnection =
    builder.Configuration.GetConnectionString("LocalIdentityConnection");

var localReliefConnection =
    builder.Configuration.GetConnectionString("LocalReliefConnection");

var demoIdentityConnection =
    builder.Configuration.GetConnectionString("DemoIdentityConnection")
    ?? "Data Source=gotg-identity-demo.db";

var demoReliefConnection =
    builder.Configuration.GetConnectionString("DemoReliefConnection")
    ?? "Data Source=gotg-relief-demo.db";

// Validate only the connections required for the selected environment.
if (isDevelopment)
{
    if (string.IsNullOrWhiteSpace(localIdentityConnection) ||
        string.IsNullOrWhiteSpace(localReliefConnection))
    {
        throw new InvalidOperationException(
            "Local development database connections are not configured.");
    }
}
else if (!isDemo && string.IsNullOrWhiteSpace(azureSqlConnection))
{
    throw new InvalidOperationException(
        "Connection string 'AzureSqlConnection' was not found.");
}

// Identity database.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (isDevelopment)
    {
        options.UseSqlServer(localIdentityConnection!);
    }
    else if (isDemo)
    {
        options.UseSqlite(demoIdentityConnection);
    }
    else
    {
        options.UseAzureSql(azureSqlConnection!);
    }
});

// Relief-management database.
if (isDemo)
{
    builder.Services.AddScoped<ReliefManagementContext>(_ =>
    {
        var options =
            new DbContextOptionsBuilder<ReliefManagementContext>()
                .UseSqlite(demoReliefConnection)
                .Options;

        return new DemoReliefManagementContext(options);
    });
}
else
{
    builder.Services.AddDbContext<ReliefManagementContext>(options =>
    {
        if (isDevelopment)
        {
            options.UseSqlServer(localReliefConnection!);
        }
        else
        {
            options.UseAzureSql(azureSqlConnection!);
        }
    });
}

// ASP.NET Core Identity.
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Initialise the databases according to the selected environment.
using (var scope = app.Services.CreateScope())
{
    if (isDevelopment)
    {
        var identityDb =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        await identityDb.Database.MigrateAsync();

        var reliefDb =
            scope.ServiceProvider
                .GetRequiredService<ReliefManagementContext>();

        await reliefDb.Database.EnsureCreatedAsync();
        await DevelopmentDataSeeder.SeedAsync(reliefDb);
    }
    else if (isDemo)
    {
        // Use EnsureCreated for the SQLite demonstration database.
        // Do not run the existing SQL Server-specific migrations here.
        var identityDb =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        await identityDb.Database.EnsureCreatedAsync();

        var reliefDb =
            scope.ServiceProvider
                .GetRequiredService<ReliefManagementContext>();

        await reliefDb.Database.EnsureCreatedAsync();
        await EnsureDemoDeliveryStatusColumnAsync(reliefDb);
        await DevelopmentDataSeeder.SeedAsync(reliefDb);
    }

    // Create development/demo roles and employee accounts when configured.
    await IdentitySeed.SeedAsync(scope.ServiceProvider);
}

// HTTP pipeline.
if (isDevelopment)
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");

    if (!isDemo)
    {
        app.UseHsts();
    }
}

// Render terminates external HTTPS. The demo container serves HTTP internally.
if (!isDemo)
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

app.Run();

// Applies small Demo-only schema updates to the SQLite relief database.
static async Task EnsureDemoDeliveryStatusColumnAsync(
    ReliefManagementContext context)
{
    var connection = context.Database.GetDbConnection();
    var openedHere =
        connection.State != System.Data.ConnectionState.Open;

    if (openedHere)
    {
        await connection.OpenAsync();
    }

    try
    {
        var columnExists = false;

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA table_info('TaxCertificates');";

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (string.Equals(
                    reader.GetString(1),
                    "DeliveryStatus",
                    StringComparison.OrdinalIgnoreCase))
                {
                    columnExists = true;
                    break;
                }
            }
        }

        if (columnExists)
        {
            Console.WriteLine(
                "Demo schema update: DeliveryStatus already exists.");
            return;
        }

        await using (var alterCommand = connection.CreateCommand())
        {
            alterCommand.CommandText =
                "ALTER TABLE \"TaxCertificates\" " +
                "ADD COLUMN \"DeliveryStatus\" TEXT " +
                "NOT NULL DEFAULT 'Pending';";

            await alterCommand.ExecuteNonQueryAsync();
        }

        Console.WriteLine(
            "Demo schema update: Added TaxCertificates.DeliveryStatus " +
            "with default Pending.");
    }
    finally
    {
        if (openedHere)
        {
            await connection.CloseAsync();
        }
    }
}
