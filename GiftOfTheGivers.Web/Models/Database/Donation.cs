using System;
using System.Collections.Generic;

namespace GiftOfTheGivers.Web.Models.Database;

public partial class Donation
{
    public long DonationId { get; set; }

    public int? DonorId { get; set; }

    public int? ReliefProjectId { get; set; }

    public int? RecurringScheduleId { get; set; }

    public decimal Amount { get; set; }

    public string CurrencyCode { get; set; } = null!;

    public string DonationType { get; set; } = null!;

    public DateTime DonationDate { get; set; }

    public bool IsAnonymous { get; set; }

    public string DonationStatus { get; set; } = null!;

    public virtual Currency CurrencyCodeNavigation { get; set; } = null!;

    public virtual Donor? Donor { get; set; }

    public virtual RecurringDonationSchedule? RecurringSchedule { get; set; }

    public virtual ReliefProject? ReliefProject { get; set; }

    public virtual TaxCertificate? TaxCertificate { get; set; }
}
