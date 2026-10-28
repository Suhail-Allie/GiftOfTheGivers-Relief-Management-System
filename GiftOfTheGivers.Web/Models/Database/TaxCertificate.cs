using System;
using System.Collections.Generic;

namespace GiftOfTheGivers.Web.Models.Database;

public partial class TaxCertificate
{
    public int TaxCertificateId { get; set; }

    public long DonationId { get; set; }

    public string CertificateNumber { get; set; } = null!;

    public DateTime GeneratedAt { get; set; }

    public string? FileReference { get; set; }

    public string CertificateStatus { get; set; } = null!;

    // Tracks delivery independently from certificate generation.
    public string DeliveryStatus { get; set; } = "Pending";

    public virtual Donation Donation { get; set; } = null!;
}
