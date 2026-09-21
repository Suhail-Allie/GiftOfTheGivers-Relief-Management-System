using System.Globalization;

namespace GiftOfTheGivers.Helpers;

public static class DonationSummaryFormatter
{
    public static string Format(
        decimal amount,
        string currency,
        string donationType)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                "Donation amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException(
                "Currency is required.",
                nameof(currency));
        }

        if (string.IsNullOrWhiteSpace(donationType))
        {
            throw new ArgumentException(
                "Donation type is required.",
                nameof(donationType));
        }

        var formattedAmount = amount.ToString(
            "N2",
            CultureInfo.InvariantCulture);

        return $"{currency.ToUpperInvariant()} {formattedAmount} - {donationType}";
    }
}