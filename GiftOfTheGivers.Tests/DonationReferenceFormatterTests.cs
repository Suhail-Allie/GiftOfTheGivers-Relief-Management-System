using GiftOfTheGivers.Helpers;

namespace GiftOfTheGivers.Tests;

public class DonationReferenceFormatterTests
{
    [Fact]
    public void Format_WithValidDonation_ReturnsExpectedReference()
    {
        var createdAt = new DateTime(
            2026,
            9,
            21,
            10,
            30,
            0,
            DateTimeKind.Utc);

        var result = DonationReferenceFormatter.Format(
            25,
            createdAt);

        Assert.Equal(
            "GOTG-DON-20260921-000025",
            result);
    }

    [Fact]
    public void Format_WithInvalidDonationId_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DonationReferenceFormatter.Format(
                0,
                DateTime.UtcNow));
    }
}