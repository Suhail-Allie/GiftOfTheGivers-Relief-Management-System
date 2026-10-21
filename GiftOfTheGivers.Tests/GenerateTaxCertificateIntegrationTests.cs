
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace GiftOfTheGivers.Tests
{
    public class GenerateTaxCertificateIntegrationTests
    {
        private static readonly HttpClient Client = new HttpClient();

        private const string BaseUrl =
            "http://localhost:7236/api/GenerateTaxCertificate";

        [Fact]
        public async Task ValidDonation_ReturnsSuccess()
        {
            // Arrange
            string url = BaseUrl +
                "?donationId=123&donorName=John";

            // Act
            using var response = await Client.GetAsync(url);
            string content =
                await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("\"success\":true", content);
            Assert.Contains("\"donationId\":123", content);
            Assert.Contains("\"donorName\":\"John\"", content);
            Assert.Contains("GOTG-TAX-", content);
        }

        [Fact]
        public async Task InvalidDonation_ReturnsBadRequest()
        {
            // Arrange
            string url = BaseUrl + "?donationId=0";

            // Act
            using var response = await Client.GetAsync(url);
            string content =
                await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest,
                response.StatusCode);

            Assert.Contains("\"success\":false", content);
        }

        [Fact]
        public async Task MissingDonorName_UsesAnonymousDonor()
        {
            // Arrange
            string url = BaseUrl + "?donationId=456";

            // Act
            using var response = await Client.GetAsync(url);
            string content =
                await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Anonymous Donor", content);
        }
    }
}
