using System;
using System.Collections.Generic;

namespace GiftOfTheGivers.Web.Models.Database;

public partial class Currency
{
    public string CurrencyCode { get; set; } = null!;

    public string CurrencyName { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    public virtual ICollection<RecurringDonationSchedule> RecurringDonationSchedules { get; set; } = new List<RecurringDonationSchedule>();
}
