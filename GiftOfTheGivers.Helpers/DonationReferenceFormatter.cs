namespace GiftOfTheGivers.Helpers;

public static class DonationReferenceFormatter
{
    public static string Format(
        int donationId,
        DateTime createdAtUtc)
    {
        if (donationId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(donationId),
                "Donation ID must be greater than zero.");
        }

        return $"GOTG-DON-{createdAtUtc:yyyyMMdd}-{donationId:D6}";
    }
}