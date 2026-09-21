using System;
using System.Collections.Generic;

namespace GiftOfTheGivers.Web.Models.Database;

public partial class VolunteerProjectAssignment
{
    public int AssignmentId { get; set; }

    public int VolunteerId { get; set; }

    public int ReliefProjectId { get; set; }

    public int? AssignedByEmployeeId { get; set; }

    public DateTime AssignedAt { get; set; }

    public string AssignmentStatus { get; set; } = null!;

    public virtual Employee? AssignedByEmployee { get; set; }

    public virtual ReliefProject ReliefProject { get; set; } = null!;

    public virtual Volunteer Volunteer { get; set; } = null!;
}
