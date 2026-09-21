using System;
using System.Collections.Generic;

namespace GiftOfTheGivers.Web.Models.Database;

public partial class ReliefProject
{
    public int ReliefProjectId { get; set; }

    public string ProjectName { get; set; } = null!;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string ProjectStatus { get; set; } = null!;

    public int CreatedByEmployeeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Employee CreatedByEmployee { get; set; } = null!;

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    public virtual ICollection<RecurringDonationSchedule> RecurringDonationSchedules { get; set; } = new List<RecurringDonationSchedule>();

    public virtual ICollection<ReliefProjectUpdate> ReliefProjectUpdates { get; set; } = new List<ReliefProjectUpdate>();

    public virtual ICollection<VolunteerProjectAssignment> VolunteerProjectAssignments { get; set; } = new List<VolunteerProjectAssignment>();
}
