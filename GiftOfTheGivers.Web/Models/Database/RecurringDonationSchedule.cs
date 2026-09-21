using System;
using System.Collections.Generic;

namespace GiftOfTheGivers.Web.Models.Database;

public partial class RecurringDonationSchedule
{
    public int RecurringScheduleId { get; set; }

    public int DonorId { get; set; }

    public int? ReliefProjectId { get; set; }

    public decimal Amount { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public string Frequency { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly? NextDonationDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    public virtual Donor Donor { get; set; } = null!;

    public virtual ReliefProject? ReliefProject { get; set; }
}
