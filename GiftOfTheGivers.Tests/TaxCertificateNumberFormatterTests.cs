using GiftOfTheGivers.Helpers;

namespace GiftOfTheGivers.Tests;

public class TaxCertificateNumberFormatterTests
{
    [Fact]
    public void Format_WithValidDonation_ReturnsExpectedCertificateNumber()
    {
        var generatedAt = new DateTime(
            2026,
            9,
            21,
            10,
            30,
            0,
            DateTimeKind.Utc);

        var result = TaxCertificateNumberFormatter.Format(
            1,
            generatedAt);

        Assert.Equal(
            "GOTG-TAX-20260921-000001",
            result);
    }

    [Fact]
    public void Format_WithInvalidDonationId_ThrowsException()
    {
        var generatedAt = DateTime.UtcNow;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            TaxCertificateNumberFormatter.Format(
                0,
                generatedAt));
    }
}