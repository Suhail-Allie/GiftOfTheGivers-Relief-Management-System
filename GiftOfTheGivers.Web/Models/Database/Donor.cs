using System;
using System.Collections.Generic;

namespace GiftOfTheGivers.Web.Models.Database;

public partial class Donor
{
    public int DonorId { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    public virtual ICollection<RecurringDonationSchedule> RecurringDonationSchedules { get; set; } = new List<RecurringDonationSchedule>();

    public virtual User User { get; set; } = null!;
}
