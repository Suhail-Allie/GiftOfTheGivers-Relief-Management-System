using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models.ViewModels;

public class DonationViewModel
{
    [Required(ErrorMessage = "Please enter a donation amount.")]
    [Range(
        typeof(decimal),
        "1.00",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Donation amount must be greater than zero.")]
    [Display(Name = "Donation amount")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Please select a currency.")]
    [Display(Name = "Currency")]
    public string CurrencyCode { get; set; } = "ZAR";

    [Required(ErrorMessage = "Please select a donation type.")]
    [Display(Name = "Donation type")]
    public string DonationType { get; set; } = "OnceOff";

    [Display(Name = "Donate anonymously")]
    public bool IsAnonymous { get; set; }

    [Display(Name = "Relief project")]
    public int? ReliefProjectId { get; set; }

    [Display(Name = "Recurring frequency")]
    public string? Frequency { get; set; }

    [Display(Name = "Start date")]
    [DataType(DataType.Date)]
    public DateOnly? StartDate { get; set; }
}