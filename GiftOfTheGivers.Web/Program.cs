using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Data.Relief;

var builder = WebApplication.CreateBuilder(args);

var isDevelopment = builder.Environment.IsDevelopment();

var azureSqlConnection =
    builder.Configuration.GetConnectionString("AzureSqlConnection");

var localIdentityConnection =
    builder.Configuration.GetConnectionString("LocalIdentityConnection");

var localReliefConnection =
    builder.Configuration.GetConnectionString("LocalReliefConnection");

// Validate the connections required for the current environment.
if (isDevelopment)
{
    if (string.IsNullOrWhiteSpace(localIdentityConnection) ||
        string.IsNullOrWhiteSpace(localReliefConnection))
    {
        throw new InvalidOperationException(
            "Local development database connections are not configured.");
    }
}
else if (string.IsNullOrWhiteSpace(azureSqlConnection))
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
    else
    {
        options.UseAzureSql(azureSqlConnection!);
    }
});

// Relief-management database.
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

// Authentication cookie routes.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// MVC.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Initialise local databases only in Development.
// Production continues to use the existing Azure SQL configuration.
using (var scope = app.Services.CreateScope())
{
    if (app.Environment.IsDevelopment())
    {
        var identityDb =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        await identityDb.Database.MigrateAsync();

        var reliefDb =
            scope.ServiceProvider
                .GetRequiredService<ReliefManagementContext>();

        // ReliefManagementContext has no migration files of its own.
        // This creates its schema in the separate local development DB.
        await reliefDb.Database.EnsureCreatedAsync();
        await DevelopmentDataSeeder.SeedAsync(reliefDb);
    }

    // Preserve the existing role and Employee identity seeding.
    await IdentitySeed.SeedAsync(scope.ServiceProvider);
}

// HTTP pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

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