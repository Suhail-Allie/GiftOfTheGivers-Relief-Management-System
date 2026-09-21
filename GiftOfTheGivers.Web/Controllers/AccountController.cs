using GiftOfTheGivers.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // =========================================================
    // LOGIN
    // =========================================================

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Employee"))
            {
                return RedirectToAction("Index", "Employee");
            }

            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email
            .Trim()
            .ToLowerInvariant();

        var user =
            await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "Invalid email address or password.");

            return View(model);
        }

        var result =
            await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            if (await _userManager.IsInRoleAsync(
                    user,
                    "Employee"))
            {
                return RedirectToAction(
                    "Index",
                    "Employee");
            }

            return RedirectToAction(
                "Index",
                "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "This account has been temporarily locked because of repeated failed login attempts.");

            return View(model);
        }

        ModelState.AddModelError(
            string.Empty,
            "Invalid email address or password.");

        return View(model);
    }

    // =========================================================
    // ACCESS DENIED
    // =========================================================

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}