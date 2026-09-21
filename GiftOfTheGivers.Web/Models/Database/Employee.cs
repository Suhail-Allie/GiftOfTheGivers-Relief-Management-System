using System;
using System.Collections.Generic;

namespace GiftOfTheGivers.Web.Models.Database;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? JobTitle { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ReliefProjectUpdate> ReliefProjectUpdates { get; set; } = new List<ReliefProjectUpdate>();

    public virtual ICollection<ReliefProject> ReliefProjects { get; set; } = new List<ReliefProject>();

    public virtual User User { get; set; } = null!;

    public virtual ICollection<VolunteerProjectAssignment> VolunteerProjectAssignments { get; set; } = new List<VolunteerProjectAssignment>();
}
