using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Web.Models.ViewModels;

public class ProjectUpdateViewModel
{
    [Required(ErrorMessage = "Please select a relief project.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a relief project.")]
    [Display(Name = "Relief project")]
    public int ReliefProjectId { get; set; }

    [Required(ErrorMessage = "Please enter an update title.")]
    [StringLength(200, ErrorMessage = "The title cannot exceed 200 characters.")]
    [Display(Name = "Update title")]
    public string UpdateTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the project update.")]
    [Display(Name = "Project update")]
    public string UpdateContent { get; set; } = string.Empty;
}