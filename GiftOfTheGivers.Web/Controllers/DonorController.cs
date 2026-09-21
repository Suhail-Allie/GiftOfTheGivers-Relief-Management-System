using GiftOfTheGivers.Web.Data.Relief;
using GiftOfTheGivers.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using DatabaseDonor = GiftOfTheGivers.Web.Models.Database.Donor;
using DatabaseUser = GiftOfTheGivers.Web.Models.Database.User;

namespace GiftOfTheGivers.Web.Controllers;

public class DonorController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ReliefManagementContext _reliefContext;

    public DonorController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        ReliefManagementContext reliefContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _reliefContext = reliefContext;
    }

    // =========================================================
    // DONOR REGISTRATION
    // =========================================================

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new DonorRegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        DonorRegisterViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email
            .Trim()
            .ToLowerInvariant();

        // -----------------------------------------------------
        // CHECK ASP.NET IDENTITY
        // -----------------------------------------------------

        var existingIdentityUser =
            await _userManager.FindByEmailAsync(email);

        if (existingIdentityUser != null)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "An account with this email address already exists.");

            return View(model);
        }

        // -----------------------------------------------------
        // CHECK RELIEF DATABASE
        // -----------------------------------------------------

        var existingDatabaseUser =
            await _reliefContext.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == email);

        if (existingDatabaseUser)
        {
            ModelState.AddModelError(
                nameof(model.Email),
                "An account with this email address already exists.");

            return View(model);
        }

        // -----------------------------------------------------
        // CREATE ASP.NET IDENTITY ACCOUNT
        // -----------------------------------------------------

        var identityUser = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var identityResult =
            await _userManager.CreateAsync(
                identityUser,
                model.Password);

        if (!identityResult.Succeeded)
        {
            foreach (var error in identityResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        // -----------------------------------------------------
        // ASSIGN DONOR ROLE
        // -----------------------------------------------------

        var roleResult =
            await _userManager.AddToRoleAsync(
                identityUser,
                "Donor");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(identityUser);

            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        // -----------------------------------------------------
        // CREATE RELIEF DATABASE USER + DONOR
        // -----------------------------------------------------

        try
        {
            var databaseUser = new DatabaseUser
            {
                Email = email,

                // Authentication remains managed by
                // ASP.NET Identity.
                PasswordHash =
                    "Authentication managed by ASP.NET Identity",

                UserRole = "Donor",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var donor = new DatabaseDonor
            {
                User = databaseUser,
                FullName = model.FullName.Trim(),
                PhoneNumber =
                    string.IsNullOrWhiteSpace(model.PhoneNumber)
                        ? null
                        : model.PhoneNumber.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _reliefContext.Users.Add(databaseUser);
            _reliefContext.Donors.Add(donor);

            // Both records are saved as one unit.
            await _reliefContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine(
                "======================================");
            Console.Error.WriteLine(
                "DONOR REGISTRATION ERROR");
            Console.Error.WriteLine(
                "======================================");
            Console.Error.WriteLine(ex.ToString());
            Console.Error.WriteLine(
                "======================================");
            Console.Error.WriteLine();

            // Remove the Identity account if the
            // relief database operation failed.
            var createdIdentityUser =
                await _userManager.FindByIdAsync(
                    identityUser.Id);

            if (createdIdentityUser != null)
            {
                await _userManager.DeleteAsync(
                    createdIdentityUser);
            }

            ModelState.AddModelError(
                string.Empty,
                "Registration could not be completed. Please try again.");

            return View(model);
        }

        // -----------------------------------------------------
        // SIGN IN SUCCESSFUL DONOR
        // -----------------------------------------------------

        await _signInManager.SignInAsync(
            identityUser,
            isPersistent: false);

        return RedirectToAction(
            nameof(RegistrationSuccess));
    }

    // =========================================================
    // REGISTRATION SUCCESS
    // =========================================================

    [HttpGet]
    public IActionResult RegistrationSuccess()
    {
        return View();
    }
}