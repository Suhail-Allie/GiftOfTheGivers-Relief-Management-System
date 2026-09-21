using GiftOfTheGivers.Helpers;
using Xunit;

namespace GiftOfTheGivers.Tests;

public class DonationSummaryFormatterTests
{
    [Fact]
    public void Format_WithValidDonation_ReturnsFormattedSummary()
    {
        var result = DonationSummaryFormatter.Format(
            1500.50m,
            "zar",
            "Once-off");

        Assert.Equal(
            "ZAR 1,500.00 - Once-off",
            result);
    }

    [Fact]
    public void Format_WithInvalidAmount_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DonationSummaryFormatter.Format(
                0,
                "ZAR",
                "Once-off"));
    }

    [Fact]
    public void Format_WithMissingCurrency_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            DonationSummaryFormatter.Format(
                100,
                "",
                "Once-off"));
    }
}