using GiftOfTheGivers.Web.Data.Relief;
using GiftOfTheGivers.Web.Models.Database;
using GiftOfTheGivers.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Controllers;

public class DonationController : Controller
{
    private readonly ReliefManagementContext _context;

    public DonationController(ReliefManagementContext context)
    {
        _context = context;
    }

    // =========================================================
    // DONATION FORM
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await LoadReliefProjectsAsync();

        return View(new DonationViewModel
        {
            CurrencyCode = "ZAR",
            DonationType = "OnceOff",
            StartDate = DateOnly.FromDateTime(DateTime.Today)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(DonationViewModel model)
    {
        var allowedCurrencies = new[]
        {
            "ZAR",
            "USD",
            "EUR"
        };

        var allowedDonationTypes = new[]
        {
            "OnceOff",
            "Recurring"
        };

        var allowedFrequencies = new[]
        {
            "Weekly",
            "Monthly",
            "Quarterly",
            "Yearly"
        };

        // -----------------------------------------------------
        // VALIDATE CURRENCY
        // -----------------------------------------------------

        if (!allowedCurrencies.Contains(model.CurrencyCode))
        {
            ModelState.AddModelError(
                nameof(model.CurrencyCode),
                "Please select a valid currency.");
        }

        // -----------------------------------------------------
        // VALIDATE DONATION TYPE
        // -----------------------------------------------------

        if (!allowedDonationTypes.Contains(model.DonationType))
        {
            ModelState.AddModelError(
                nameof(model.DonationType),
                "Please select a valid donation type.");
        }

        // -----------------------------------------------------
        // VALIDATE RELIEF PROJECT
        // -----------------------------------------------------

        if (model.ReliefProjectId.HasValue)
        {
            var projectExists = await _context.ReliefProjects
                .AsNoTracking()
                .AnyAsync(p =>
                    p.ReliefProjectId == model.ReliefProjectId.Value &&
                    p.ProjectStatus == "Active");

            if (!projectExists)
            {
                ModelState.AddModelError(
                    nameof(model.ReliefProjectId),
                    "Please select a valid active relief project.");
            }
        }

        // -----------------------------------------------------
        // FIND LOGGED-IN DONOR
        // -----------------------------------------------------

        Donor? donor = null;

        if (User.Identity?.IsAuthenticated == true &&
            !string.IsNullOrWhiteSpace(User.Identity.Name))
        {
            var email = User.Identity.Name;

            donor = await _context.Donors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d =>
                    d.User.Email == email);
        }

        // -----------------------------------------------------
        // RECURRING DONATION RULES
        // -----------------------------------------------------

        if (model.DonationType == "Recurring")
        {
            if (!User.IsInRole("Donor") || donor == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Recurring donations require a registered donor account.");
            }

            if (string.IsNullOrWhiteSpace(model.Frequency) ||
                !allowedFrequencies.Contains(model.Frequency))
            {
                ModelState.AddModelError(
                    nameof(model.Frequency),
                    "Please select a recurring frequency.");
            }

            if (!model.StartDate.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.StartDate),
                    "Please select a start date.");
            }
            else if (model.StartDate.Value <
                     DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError(
                    nameof(model.StartDate),
                    "The start date cannot be in the past.");
            }
        }

        // -----------------------------------------------------
        // RETURN VALIDATION ERRORS
        // -----------------------------------------------------

        if (!ModelState.IsValid)
        {
            await LoadReliefProjectsAsync();

            return View(model);
        }

        // -----------------------------------------------------
        // CREATE ONCE-OFF DONATION
        // -----------------------------------------------------

        if (model.DonationType == "OnceOff")
        {
            var anonymousDonation =
                model.IsAnonymous || donor == null;

            var donation = new Donation
            {
                DonorId = anonymousDonation
                    ? null
                    : donor!.DonorId,

                ReliefProjectId = model.ReliefProjectId,
                RecurringScheduleId = null,
                Amount = model.Amount,
                CurrencyCode = model.CurrencyCode,
                DonationType = "OnceOff",
                DonationDate = DateTime.UtcNow,
                IsAnonymous = anonymousDonation,
                DonationStatus = "Completed"
            };

            var certificate =
                CreatePlaceholderTaxCertificate(donation);

            _context.Donations.Add(donation);
            _context.TaxCertificates.Add(certificate);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Confirmation),
                new { id = donation.DonationId });
        }

        // -----------------------------------------------------
        // CREATE RECURRING DONATION
        // -----------------------------------------------------

        var startDate = model.StartDate!.Value;

        var recurringSchedule =
            new RecurringDonationSchedule
            {
                DonorId = donor!.DonorId,
                ReliefProjectId = model.ReliefProjectId,
                Amount = model.Amount,
                CurrencyCode = model.CurrencyCode,
                Frequency = model.Frequency!,
                StartDate = startDate,
                NextDonationDate = CalculateNextDate(
                    startDate,
                    model.Frequency!),
                EndDate = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

        var recurringDonation = new Donation
        {
            DonorId = donor.DonorId,
            ReliefProjectId = model.ReliefProjectId,
            RecurringSchedule = recurringSchedule,
            Amount = model.Amount,
            CurrencyCode = model.CurrencyCode,
            DonationType = "Recurring",
            DonationDate = DateTime.UtcNow,
            IsAnonymous = false,
            DonationStatus = "Completed"
        };

        var recurringCertificate =
            CreatePlaceholderTaxCertificate(
                recurringDonation);

        _context.RecurringDonationSchedules.Add(
            recurringSchedule);

        _context.Donations.Add(
            recurringDonation);

        _context.TaxCertificates.Add(
            recurringCertificate);

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Confirmation),
            new { id = recurringDonation.DonationId });
    }

    // =========================================================
    // DONATION CONFIRMATION
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Confirmation(long id)
    {
        var donation = await _context.Donations
            .AsNoTracking()
            .Include(d => d.ReliefProject)
            .Include(d => d.RecurringSchedule)
            .Include(d => d.TaxCertificate)
            .FirstOrDefaultAsync(d =>
                d.DonationId == id);

        if (donation == null)
        {
            return NotFound();
        }

        return View(donation);
    }

    // =========================================================
    // TAX CERTIFICATE
    // =========================================================

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> TaxCertificate(long id)
    {
        var donation = await _context.Donations
            .Include(d => d.Donor)
                .ThenInclude(donor => donor!.User)
            .Include(d => d.ReliefProject)
            .Include(d => d.TaxCertificate)
            .FirstOrDefaultAsync(d =>
                d.DonationId == id);

        if (donation == null)
        {
            return NotFound();
        }

        // -----------------------------------------------------
        // REGISTERED DONATION SECURITY
        // -----------------------------------------------------

        if (!donation.IsAnonymous)
        {
            if (User.Identity?.IsAuthenticated != true ||
                string.IsNullOrWhiteSpace(User.Identity.Name))
            {
                return Challenge();
            }

            if (!User.IsInRole("Donor") ||
                donation.Donor == null)
            {
                return Forbid();
            }

            var currentEmail =
                User.Identity.Name;

            if (!string.Equals(
                    donation.Donor.User.Email,
                    currentEmail,
                    StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }
        }

        // -----------------------------------------------------
        // LEGACY/FALLBACK CERTIFICATE GENERATION
        // -----------------------------------------------------

        if (donation.TaxCertificate == null)
        {
            var certificate =
                CreatePlaceholderTaxCertificate(
                    donation);

            _context.TaxCertificates.Add(
                certificate);

            await _context.SaveChangesAsync();

            donation.TaxCertificate =
                certificate;
        }

        return View(
            donation.TaxCertificate);
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private async Task LoadReliefProjectsAsync()
    {
        var projects = await _context.ReliefProjects
            .AsNoTracking()
            .Where(p => p.ProjectStatus == "Active")
            .OrderBy(p => p.ProjectName)
            .ToListAsync();

        ViewBag.ReliefProjects =
            new SelectList(
                projects,
                "ReliefProjectId",
                "ProjectName");
    }

    private static DateOnly CalculateNextDate(
        DateOnly startDate,
        string frequency)
    {
        return frequency switch
        {
            "Weekly" =>
                startDate.AddDays(7),

            "Monthly" =>
                startDate.AddMonths(1),

            "Quarterly" =>
                startDate.AddMonths(3),

            "Yearly" =>
                startDate.AddYears(1),

            _ => startDate
        };
    }

    private static TaxCertificate
        CreatePlaceholderTaxCertificate(
            Donation donation)
    {
        var certificate =
            new TaxCertificate
            {
                CertificateNumber =
                    GenerateCertificateNumber(),

                GeneratedAt =
                    DateTime.UtcNow,

                FileReference =
                    null,

                CertificateStatus =
                    "Generated",

                Donation =
                    donation
            };

        donation.TaxCertificate =
            certificate;

        return certificate;
    }

    private static string GenerateCertificateNumber()
    {
        var datePart =
            DateTime.UtcNow
                .ToString("yyyyMMdd");

        var uniquePart =
            Guid.NewGuid()
                .ToString("N")[..8]
                .ToUpperInvariant();

        return
            $"GOTG-TC-{datePart}-{uniquePart}";
    }
}