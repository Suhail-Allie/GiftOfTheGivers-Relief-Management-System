using GiftOfTheGivers.Web.Data.Relief;
using GiftOfTheGivers.Web.Models.Database;
using GiftOfTheGivers.Web.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GiftOfTheGivers.Web.Controllers;

public class VolunteerController : Controller
{
    private readonly ReliefManagementContext _context;

    public VolunteerController(ReliefManagementContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new VolunteerInterestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(VolunteerInterestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var volunteer = new Volunteer
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)
                ? null
                : model.PhoneNumber.Trim(),
            Skills = model.Skills.Trim(),
            Availability = model.Availability.Trim(),
            RegistrationDate = DateTime.UtcNow,
            VolunteerStatus = "Pending"
        };

        _context.Volunteers.Add(volunteer);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Confirmation));
    }

    [HttpGet]
    public IActionResult Confirmation()
    {
        return View();
    }
}