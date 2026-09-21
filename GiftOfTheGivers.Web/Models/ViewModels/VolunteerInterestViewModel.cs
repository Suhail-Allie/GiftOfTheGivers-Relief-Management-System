using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models.ViewModels;

public class VolunteerInterestViewModel
{
    [Required(ErrorMessage = "Please enter your full name.")]
    [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters.")]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your email address.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(30, ErrorMessage = "Phone number cannot exceed 30 characters.")]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Please describe your skills.")]
    [StringLength(500, ErrorMessage = "Skills cannot exceed 500 characters.")]
    public string Skills { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide your availability.")]
    [StringLength(250, ErrorMessage = "Availability cannot exceed 250 characters.")]
    public string Availability { get; set; } = string.Empty;
}