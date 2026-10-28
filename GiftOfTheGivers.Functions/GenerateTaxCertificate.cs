using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GiftOfTheGivers.Functions;

public class GenerateTaxCertificate
{
    private readonly ILogger<GenerateTaxCertificate> _logger;

    public GenerateTaxCertificate(ILogger<GenerateTaxCertificate> logger)
    {
        _logger = logger;
    }

    [Function("GenerateTaxCertificate")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
        HttpRequestData req)
    {
        _logger.LogInformation(
            "GenerateTaxCertificate function received a request.");

        try
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);

            var donationIdValue = query["donationId"];
            var donorName = query["donorName"];

            if (!int.TryParse(donationIdValue, out var donationId) ||
                donationId <= 0)
            {
                _logger.LogWarning(
                    "Invalid donationId supplied: {DonationId}",
                    donationIdValue);

                var badRequest = req.CreateResponse(
                    HttpStatusCode.BadRequest);

                await badRequest.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "A valid positive donationId is required."
                });

                return badRequest;
            }

            if (string.IsNullOrWhiteSpace(donorName))
            {
                donorName = "Anonymous Donor";
            }

            _logger.LogInformation(
                "Generating dummy tax certificate for DonationID {DonationId} for {DonorName}.",
                donationId,
                donorName);

            var certificateNumber =
                $"GOTG-TAX-{DateTime.UtcNow:yyyyMMdd}-{donationId:D6}";

            var response = req.CreateResponse(
                HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new
            {
                success = true,
                donationId,
                donorName,
                certificateNumber,
                status = "Generated",
                generatedAt = DateTime.UtcNow,
                message = "Dummy tax certificate generated successfully."
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while generating the tax certificate.");

            var errorResponse = req.CreateResponse(
                HttpStatusCode.InternalServerError);

            await errorResponse.WriteAsJsonAsync(new
            {
                success = false,
                message =
                    "An unexpected error occurred while generating the tax certificate."
            });

            return errorResponse;
        }
    }
}
