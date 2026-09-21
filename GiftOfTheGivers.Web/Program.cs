using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GiftOfTheGivers.Web.Data;
using GiftOfTheGivers.Web.Data.Relief;

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// AZURE SQL CONNECTION
// =========================================================

var azureSqlConnection = builder.Configuration
    .GetConnectionString("AzureSqlConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'AzureSqlConnection' was not found.");

// =========================================================
// ASP.NET IDENTITY DATABASE
// =========================================================

// ASP.NET Identity now uses the same Azure SQL database
// as the relief-management data.
//
// Identity creates its own AspNet* tables and does not
// replace the existing domain Users table.

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseAzureSql(azureSqlConnection));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// =========================================================
// RELIEF MANAGEMENT DATABASE
// =========================================================

builder.Services.AddDbContext<ReliefManagementContext>(options =>
    options.UseAzureSql(azureSqlConnection));

// =========================================================
// ASP.NET IDENTITY
// =========================================================

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

// =========================================================
// AUTHENTICATION COOKIE ROUTES
// =========================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// =========================================================
// MVC
// =========================================================

builder.Services.AddControllersWithViews();

var app = builder.Build();

// =========================================================
// SEED IDENTITY ROLES AND EMPLOYEE ACCOUNT
// =========================================================

using (var scope = app.Services.CreateScope())
{
    await IdentitySeed.SeedAsync(scope.ServiceProvider);
}

// =========================================================
// HTTP PIPELINE
// =========================================================

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