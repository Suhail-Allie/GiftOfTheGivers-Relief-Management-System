using GiftOfTheGivers.Web.Data.Relief;
using GiftOfTheGivers.Web.Models.Database;
using GiftOfTheGivers.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Controllers;

[Authorize(Roles = "Employee")]
public class EmployeeController : Controller
{
    private readonly ReliefManagementContext _context;

    public EmployeeController(ReliefManagementContext context)
    {
        _context = context;
    }

    // =========================================================
    // EMPLOYEE DASHBOARD
    // =========================================================

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // =========================================================
    // VOLUNTEER REGISTRATIONS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Volunteers()
    {
        var volunteers = await _context.Volunteers
            .AsNoTracking()
            .OrderByDescending(v => v.RegistrationDate)
            .ToListAsync();

        return View(volunteers);
    }

    // =========================================================
    // RELIEF PROJECT UPDATES
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> ProjectUpdates()
    {
        await LoadReliefProjectsAsync();

        return View(new ProjectUpdateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProjectUpdates(
        ProjectUpdateViewModel model)
    {
        var projectExists = await _context.ReliefProjects
            .AsNoTracking()
            .AnyAsync(p =>
                p.ReliefProjectId == model.ReliefProjectId &&
                p.ProjectStatus == "Active");

        if (!projectExists)
        {
            ModelState.AddModelError(
                nameof(model.ReliefProjectId),
                "Please select an active relief project.");
        }

        var currentEmail = User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(currentEmail))
        {
            return Challenge();
        }

        var employee = await _context.Employees
            .Include(e => e.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.User.Email == currentEmail);

        if (employee == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "A matching Employee profile could not be found.");
        }

        if (!ModelState.IsValid)
        {
            await LoadReliefProjectsAsync();

            return View(model);
        }

        var projectUpdate = new ReliefProjectUpdate
        {
            ReliefProjectId = model.ReliefProjectId,
            EmployeeId = employee!.EmployeeId,
            UpdateTitle = model.UpdateTitle.Trim(),
            UpdateContent = model.UpdateContent.Trim(),
            PostedAt = DateTime.UtcNow
        };

        _context.ReliefProjectUpdates.Add(projectUpdate);

        await _context.SaveChangesAsync();

        TempData["ProjectUpdateSuccess"] =
            "The relief project update was posted successfully.";

        return RedirectToAction(nameof(ProjectUpdates));
    }

    // =========================================================
    // PRIVATE HELPERS
    // =========================================================

    private async Task LoadReliefProjectsAsync()
    {
        var projects = await _context.ReliefProjects
            .AsNoTracking()
            .Where(p => p.ProjectStatus == "Active")
            .OrderBy(p => p.ProjectName)
            .ToListAsync();

        ViewBag.ReliefProjects = new SelectList(
            projects,
            "ReliefProjectId",
            "ProjectName");
    }
}