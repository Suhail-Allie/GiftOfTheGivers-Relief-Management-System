namespace GiftOfTheGivers.Helpers
{
    public static class DonationHelper
    {
        public static string GenerateCertificateNumber()
        {
            string date = DateTime.Now.ToString("yyyyMMdd");

            string uniqueCode = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpper();

            return $"GOTG-{date}-{uniqueCode}";
        }

        public static decimal CalculateDonationTotal(
            decimal donationAmount,
            decimal additionalAmount)
        {
            return donationAmount + additionalAmount;
        }
    }
}