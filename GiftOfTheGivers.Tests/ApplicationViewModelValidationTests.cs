using System.ComponentModel.DataAnnotations;
using GiftOfTheGivers.Web.Models.ViewModels;

namespace GiftOfTheGivers.Tests;

public class ApplicationViewModelValidationTests
{
    private static List<ValidationResult> ValidateModel(object model)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            model,
            new ValidationContext(model),
            results,
            validateAllProperties: true);

        return results;
    }

    // DONATION VIEW MODEL TESTS

    [Fact]
    public void Donation_WithValidDetails_PassesValidation()
    {
        var model = new DonationViewModel
        {
            Amount = 1500.50m,
            CurrencyCode = "ZAR",
            DonationType = "OnceOff",
            IsAnonymous = true
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Donation_WithZeroAmount_FailsValidation()
    {
        var model = new DonationViewModel
        {
            Amount = 0m,
            CurrencyCode = "ZAR",
            DonationType = "OnceOff"
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(DonationViewModel.Amount)));
    }

    [Fact]
    public void Donation_WithNegativeAmount_FailsValidation()
    {
        var model = new DonationViewModel
        {
            Amount = -100m,
            CurrencyCode = "ZAR",
            DonationType = "OnceOff"
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(DonationViewModel.Amount)));
    }

    [Fact]
    public void Donation_AboveMaximumAmount_FailsValidation()
    {
        var model = new DonationViewModel
        {
            Amount = 10000000000000000m,
            CurrencyCode = "ZAR",
            DonationType = "OnceOff"
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(DonationViewModel.Amount)));
    }

    [Fact]
    public void Donation_WithoutCurrency_FailsValidation()
    {
        var model = new DonationViewModel
        {
            Amount = 100m,
            CurrencyCode = string.Empty,
            DonationType = "OnceOff"
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(DonationViewModel.CurrencyCode)));
    }

    [Fact]
    public void Donation_WithoutDonationType_FailsValidation()
    {
        var model = new DonationViewModel
        {
            Amount = 100m,
            CurrencyCode = "ZAR",
            DonationType = string.Empty
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(DonationViewModel.DonationType)));
    }

    // VOLUNTEER VIEW MODEL TESTS

    [Fact]
    public void Volunteer_WithValidDetails_PassesValidation()
    {
        var model = new VolunteerInterestViewModel
        {
            FullName = "Test Volunteer",
            Email = "volunteer@example.com",
            PhoneNumber = "0821234567",
            Skills = "First aid and logistics",
            Availability = "Weekends"
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void Volunteer_WithoutFullName_FailsValidation()
    {
        var model = new VolunteerInterestViewModel
        {
            FullName = string.Empty,
            Email = "volunteer@example.com",
            Skills = "First aid",
            Availability = "Weekends"
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(VolunteerInterestViewModel.FullName)));
    }

    [Fact]
    public void Volunteer_WithInvalidEmail_FailsValidation()
    {
        var model = new VolunteerInterestViewModel
        {
            FullName = "Test Volunteer",
            Email = "not-a-valid-email",
            Skills = "First aid",
            Availability = "Weekends"
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(VolunteerInterestViewModel.Email)));
    }

    [Fact]
    public void Volunteer_WithoutSkills_FailsValidation()
    {
        var model = new VolunteerInterestViewModel
        {
            FullName = "Test Volunteer",
            Email = "volunteer@example.com",
            Skills = string.Empty,
            Availability = "Weekends"
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(VolunteerInterestViewModel.Skills)));
    }

    [Fact]
    public void Volunteer_WithoutAvailability_FailsValidation()
    {
        var model = new VolunteerInterestViewModel
        {
            FullName = "Test Volunteer",
            Email = "volunteer@example.com",
            Skills = "First aid",
            Availability = string.Empty
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(VolunteerInterestViewModel.Availability)));
    }

    [Fact]
    public void Volunteer_WithNameAboveMaximumLength_FailsValidation()
    {
        var model = new VolunteerInterestViewModel
        {
            FullName = new string('A', 151),
            Email = "volunteer@example.com",
            Skills = "First aid",
            Availability = "Weekends"
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(VolunteerInterestViewModel.FullName)));
    }

    [Fact]
    public void Volunteer_WithSkillsAboveMaximumLength_FailsValidation()
    {
        var model = new VolunteerInterestViewModel
        {
            FullName = "Test Volunteer",
            Email = "volunteer@example.com",
            Skills = new string('A', 501),
            Availability = "Weekends"
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(VolunteerInterestViewModel.Skills)));
    }

    [Fact]
    public void Volunteer_WithInvalidPhoneNumber_FailsValidation()
    {
        var model = new VolunteerInterestViewModel
        {
            FullName = "Test Volunteer",
            Email = "volunteer@example.com",
            PhoneNumber = "not-a-phone",
            Skills = "First aid",
            Availability = "Weekends"
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(VolunteerInterestViewModel.PhoneNumber)));
    }

    // RELIEF PROJECT UPDATE VIEW MODEL TESTS

    [Fact]
    public void ProjectUpdate_WithValidDetails_PassesValidation()
    {
        var model = new ProjectUpdateViewModel
        {
            ReliefProjectId = 1,
            UpdateTitle = "Relief supplies delivered",
            UpdateContent =
                "Supplies were delivered to the affected community."
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Fact]
    public void ProjectUpdate_WithInvalidProjectId_FailsValidation()
    {
        var model = new ProjectUpdateViewModel
        {
            ReliefProjectId = 0,
            UpdateTitle = "Relief update",
            UpdateContent = "Supplies were delivered."
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(ProjectUpdateViewModel.ReliefProjectId)));
    }

    [Fact]
    public void ProjectUpdate_WithoutTitle_FailsValidation()
    {
        var model = new ProjectUpdateViewModel
        {
            ReliefProjectId = 1,
            UpdateTitle = string.Empty,
            UpdateContent = "Supplies were delivered."
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(ProjectUpdateViewModel.UpdateTitle)));
    }

    [Fact]
    public void ProjectUpdate_WithTitleAboveMaximumLength_FailsValidation()
    {
        var model = new ProjectUpdateViewModel
        {
            ReliefProjectId = 1,
            UpdateTitle = new string('A', 201),
            UpdateContent = "Supplies were delivered."
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(ProjectUpdateViewModel.UpdateTitle)));
    }

    [Fact]
    public void ProjectUpdate_WithoutContent_FailsValidation()
    {
        var model = new ProjectUpdateViewModel
        {
            ReliefProjectId = 1,
            UpdateTitle = "Relief update",
            UpdateContent = string.Empty
        };

        var results = ValidateModel(model);

        Assert.Contains(
            results,
            result => result.MemberNames.Contains(
                nameof(ProjectUpdateViewModel.UpdateContent)));
    }
}