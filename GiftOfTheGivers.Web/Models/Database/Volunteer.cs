using System;
using System.Collections.Generic;

namespace GiftOfTheGivers.Web.Models.Database;

public partial class Volunteer
{
    public int VolunteerId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string Skills { get; set; } = null!;

    public string Availability { get; set; } = null!;

    public DateTime RegistrationDate { get; set; }

    public string VolunteerStatus { get; set; } = null!;

    public virtual ICollection<VolunteerProjectAssignment> VolunteerProjectAssignments { get; set; } = new List<VolunteerProjectAssignment>();
}
