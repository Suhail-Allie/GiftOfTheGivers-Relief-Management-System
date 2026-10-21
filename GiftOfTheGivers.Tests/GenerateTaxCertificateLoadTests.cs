
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace GiftOfTheGivers.Tests
{
    public class GenerateTaxCertificateLoadTests
    {
        private readonly ITestOutputHelper _output;

        private static readonly HttpClient Client =
            new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

        private const string BaseUrl =
            "http://localhost:7236/api/GenerateTaxCertificate";

        public GenerateTaxCertificateLoadTests(
            ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task FiveConcurrentUsers_AllRequestsSucceed()
        {
            const int concurrentUsers = 5;

            var totalTimer = Stopwatch.StartNew();

            // Create five HTTP requests concurrently.
            var tasks = Enumerable.Range(1, concurrentUsers)
                .Select(async userNumber =>
                {
                    string url = BaseUrl +
                        $"?donationId={1000 + userNumber}" +
                        $"&donorName=LoadUser{userNumber}";

                    var timer = Stopwatch.StartNew();

                    using var response =
                        await Client.GetAsync(url);

                    string body =
                        await response.Content.ReadAsStringAsync();

                    timer.Stop();

                    return new
                    {
                        User = userNumber,
                        StatusCode = response.StatusCode,
                        DurationMs = timer.ElapsedMilliseconds,
                        ValidResponse =
                            body.Contains("\"success\":true")
                    };
                });

            var results = await Task.WhenAll(tasks);

            totalTimer.Stop();

            foreach (var result in results.OrderBy(r => r.User))
            {
                _output.WriteLine(
                    $"User {result.User}: " +
                    $"HTTP {(int)result.StatusCode}, " +
                    $"{result.DurationMs} ms, " +
                    $"Valid response: {result.ValidResponse}");
            }

            _output.WriteLine(
                $"Total elapsed: {totalTimer.ElapsedMilliseconds} ms");

            _output.WriteLine(
                $"Average response time: " +
                $"{results.Average(r => r.DurationMs):F2} ms");

            _output.WriteLine(
                $"Successful requests: " +
                $"{results.Count(r =>
                    r.StatusCode == HttpStatusCode.OK &&
                    r.ValidResponse)}/{concurrentUsers}");

            Assert.All(results, result =>
            {
                Assert.Equal(
                    HttpStatusCode.OK,
                    result.StatusCode);

                Assert.True(result.ValidResponse);
            });
        }
    }
}
