namespace GiftOfTheGivers.Helpers;

public static class TaxCertificateNumberFormatter
{
    public static string Format(int donationId, DateTime generatedAtUtc)
    {
        if (donationId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(donationId),
                "Donation ID must be greater than zero.");
        }

        return $"GOTG-TAX-{generatedAtUtc:yyyyMMdd}-{donationId:D6}";
    }
}