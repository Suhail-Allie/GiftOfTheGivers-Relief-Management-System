using System.Collections.Generic;
using GiftOfTheGivers.Web.Controllers;
using GiftOfTheGivers.Web.Data.Relief;
using GiftOfTheGivers.Web.Models.Database;
using GiftOfTheGivers.Web.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Tests;

public class DonationControllerIntegrationTests
{
    private static async Task<(
        SqliteConnection Connection,
        TestReliefManagementContext Context)>
        CreateTestDatabaseAsync()
    {
        var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<ReliefManagementContext>()
                .UseSqlite(connection)
                .Options;

        var context = new TestReliefManagementContext(options);

        await context.Database.EnsureCreatedAsync();

        // Seed the currencies required by the donation records.
        context.Currencies.AddRange(
            new Currency
            {
                CurrencyCode = "ZAR",
                CurrencyName = "South African Rand",
                IsActive = true
            },
            new Currency
            {
                CurrencyCode = "USD",
                CurrencyName = "US Dollar",
                IsActive = true
            },
            new Currency
            {
                CurrencyCode = "EUR",
                CurrencyName = "Euro",
                IsActive = true
            });

        await context.SaveChangesAsync();

        return (connection, context);
    }

    private static DonationController CreateController(
        TestReliefManagementContext context)
    {
        var controller = new DonationController(context);
        var httpContext = new DefaultHttpContext();

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.TempData = new TempDataDictionary(
            httpContext,
            new TestTempDataProvider());

        return controller;
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(
            HttpContext context)
        {
            return new Dictionary<string, object>();
        }

        public void SaveTempData(
            HttpContext context,
            IDictionary<string, object> values)
        {
        }
    }
    private static DonationViewModel ValidAnonymousDonation()
    {
        return new DonationViewModel
        {
            Amount = 250.00m,
            CurrencyCode = "ZAR",
            DonationType = "OnceOff",
            IsAnonymous = true
        };
    }

    [Fact]
    public async Task Index_ValidAnonymousDonation_SavesDonationAndCertificate()
    {
        var (connection, context) =
            await CreateTestDatabaseAsync();

        await using var connectionScope = connection;
        await using var contextScope = context;

        var controller = CreateController(context);
        var model = ValidAnonymousDonation();

        var result = await controller.Index(model);

        var redirect =
            Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(
            nameof(DonationController.Confirmation),
            redirect.ActionName);

        // Read the records back from SQLite.
        var donation = await context.Donations
            .Include(d => d.TaxCertificate)
            .SingleAsync();

        Assert.True(donation.DonationId > 0);
        Assert.Equal(250.00m, donation.Amount);
        Assert.Equal("ZAR", donation.CurrencyCode);
        Assert.Equal("OnceOff", donation.DonationType);
        Assert.Equal("Completed", donation.DonationStatus);
        Assert.True(donation.IsAnonymous);
        Assert.Null(donation.DonorId);

        // The certificate must be persisted and linked to the donation.
        Assert.NotNull(donation.TaxCertificate);

        Assert.Equal(
            donation.DonationId,
            donation.TaxCertificate!.DonationId);

        Assert.StartsWith(
            "GOTG-TC-",
            donation.TaxCertificate.CertificateNumber);

        Assert.Equal(
            "Generated",
            donation.TaxCertificate.CertificateStatus);
    }

    [Fact]
    public async Task Index_InvalidCurrency_DoesNotSaveDonation()
    {
        var (connection, context) =
            await CreateTestDatabaseAsync();

        await using var connectionScope = connection;
        await using var contextScope = context;

        var controller = CreateController(context);
        var model = ValidAnonymousDonation();

        // GBP is deliberately excluded from the application's
        // supported currencies.
        model.CurrencyCode = "GBP";

        var result = await controller.Index(model);

        Assert.IsType<ViewResult>(result);

        Assert.False(controller.ModelState.IsValid);

        Assert.True(
            controller.ModelState.ContainsKey(
                nameof(DonationViewModel.CurrencyCode)));

        Assert.Equal(0, await context.Donations.CountAsync());
        Assert.Equal(0, await context.TaxCertificates.CountAsync());
    }

    [Fact]
    public async Task Confirmation_ExistingDonation_ReturnsDonationAndCertificate()
    {
        var (connection, context) =
            await CreateTestDatabaseAsync();

        await using var connectionScope = connection;
        await using var contextScope = context;

        var controller = CreateController(context);

        // First, complete the normal anonymous donation workflow.
        await controller.Index(ValidAnonymousDonation());

        var savedDonation = await context.Donations
            .AsNoTracking()
            .SingleAsync();

        // Then retrieve the confirmation page for the saved donation.
        var result = await controller.Confirmation(
            savedDonation.DonationId);

        var viewResult = Assert.IsType<ViewResult>(result);

        var confirmationDonation =
            Assert.IsType<Donation>(viewResult.Model);

        Assert.Equal(
            savedDonation.DonationId,
            confirmationDonation.DonationId);

        Assert.NotNull(confirmationDonation.TaxCertificate);

        Assert.Equal(
            "Generated",
            confirmationDonation.TaxCertificate!.CertificateStatus);
    }
}

// This test-only context removes SQL Server-specific database
// defaults so the same entity model can be created in SQLite.
// Production database configuration is not changed.
internal sealed class TestReliefManagementContext
    : ReliefManagementContext
{
    public TestReliefManagementContext(
        DbContextOptions<ReliefManagementContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                property.SetDefaultValueSql(null);
                property.SetDefaultValue(null);
            }
        }
    }
}

