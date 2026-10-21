
using System;
using GiftOfTheGivers.Helpers;
using Xunit;

namespace GiftOfTheGivers.Tests
{
    public class DonationHelperTests
    {
        [Fact]
        public void CalculateDonationTotal_AddsAmountsCorrectly()
        {
            // Arrange
            decimal donationAmount = 500m;
            decimal additionalAmount = 100m;

            // Act
            decimal result = DonationHelper.CalculateDonationTotal(
                donationAmount, additionalAmount);

            // Assert
            Assert.Equal(600m, result);
        }

        [Fact]
        public void CalculateDonationTotal_HandlesZeroAmount()
        {
            // Arrange
            decimal donationAmount = 0m;
            decimal additionalAmount = 0m;

            // Act
            decimal result = DonationHelper.CalculateDonationTotal(
                donationAmount, additionalAmount);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public void GenerateCertificateNumber_HasCorrectFormat()
        {
            // Act
            string certificateNumber =
                DonationHelper.GenerateCertificateNumber();

            // Assert
            Assert.Matches(
                @"^GOTG-\d{8}-[A-F0-9]{8}$",
                certificateNumber);
        }

        [Fact]
        public void GenerateCertificateNumber_CreatesUniqueNumbers()
        {
            // Act
            string firstNumber =
                DonationHelper.GenerateCertificateNumber();

            string secondNumber =
                DonationHelper.GenerateCertificateNumber();

            // Assert
            Assert.NotEqual(firstNumber, secondNumber);
        }
    }
}
