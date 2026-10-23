using System;
using System.IO;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace GiftOfTheGivers.UI.Tests;

public class SeleniumUiTests
{
    private const string DefaultBaseUrl = "http://localhost:5080";

    private static string BaseUrl =>
        (Environment.GetEnvironmentVariable("GOTG_TEST_BASE_URL")
         ?? DefaultBaseUrl).TrimEnd('/');

    private static ChromeDriver CreateDriver()
    {
        var options = new ChromeOptions();

        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1440,1200");
        options.AddArgument("--disable-gpu");

        var driver = new ChromeDriver(options);
        driver.Manage().Timeouts().PageLoad =
            TimeSpan.FromSeconds(30);

        return driver;
    }

    private static WebDriverWait CreateWait(IWebDriver driver)
    {
        return new WebDriverWait(
            driver,
            TimeSpan.FromSeconds(20));
    }

    private static void SaveEvidence(
        IWebDriver driver,
        string name)
    {
        var current =
            new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current != null &&
               !File.Exists(Path.Combine(
                   current.FullName,
                   "GiftOfTheGivers.slnx")))
        {
            current = current.Parent;
        }

        var root =
            current?.FullName ?? Directory.GetCurrentDirectory();

        var directory = Path.Combine(
            root,
            "Task3-Evidence",
            "Selenium");

        Directory.CreateDirectory(directory);

        var filename = Path.Combine(
            directory,
            $"{name}_{DateTime.Now:yyyyMMdd_HHmmss}.png");

        ((ITakesScreenshot)driver)
            .GetScreenshot()
            .SaveAsFile(filename);

        Console.WriteLine(
            $"Selenium screenshot saved: {filename}");
    }

    [Fact]
    public void DonationPage_LoadsWithExpectedFields()
    {
        using var driver = CreateDriver();
        var wait = CreateWait(driver);

        driver.Navigate().GoToUrl(
            $"{BaseUrl}/Donation/Index");

        var amount = wait.Until(
            d => d.FindElement(By.Id("Amount")));

        Assert.True(amount.Displayed);

        var currency = new SelectElement(
            driver.FindElement(By.Id("CurrencyCode")));

        Assert.Contains(
            currency.Options,
            option => option.GetAttribute("value") == "ZAR");

        Assert.Contains(
            currency.Options,
            option => option.GetAttribute("value") == "USD");

        Assert.Contains(
            currency.Options,
            option => option.GetAttribute("value") == "EUR");

        Assert.True(
            driver.FindElement(By.Id("DonationType")).Displayed);

        Assert.True(
            driver.FindElement(By.Id("IsAnonymous")).Displayed);

        SaveEvidence(driver, "Donation_Form_Loads");
    }

    [Fact]
    public void VolunteerForm_SubmitsSuccessfully()
    {
        using var driver = CreateDriver();
        var wait = CreateWait(driver);

        driver.Navigate().GoToUrl(
            $"{BaseUrl}/Volunteer/Index");

        wait.Until(
            d => d.FindElement(By.Id("FullName")));

        var testId = Guid.NewGuid().ToString("N")[..10];

        driver.FindElement(By.Id("FullName"))
            .SendKeys($"Selenium Test Volunteer {testId}");

        driver.FindElement(By.Id("Email"))
            .SendKeys($"selenium-{testId}@example.com");

        driver.FindElement(By.Id("PhoneNumber"))
            .SendKeys("0712345678");

        driver.FindElement(By.Id("Skills"))
            .SendKeys("Selenium automated testing");

        driver.FindElement(By.Id("Availability"))
            .SendKeys("Available for local testing");

        driver.FindElement(
            By.CssSelector("form button[type='submit']"))
            .Click();

        wait.Until(d =>
            d.Url.Contains(
                "/Volunteer/Confirmation",
                StringComparison.OrdinalIgnoreCase));

        Assert.Contains(
            "/Volunteer/Confirmation",
            driver.Url,
            StringComparison.OrdinalIgnoreCase);

        SaveEvidence(
            driver,
            "Volunteer_Registration_Confirmation");
    }

    [Fact]
    public void AnonymousDonation_SubmitsSuccessfully()
    {
        using var driver = CreateDriver();
        var wait = CreateWait(driver);

        driver.Navigate().GoToUrl(
            $"{BaseUrl}/Donation/Index");

        wait.Until(
            d => d.FindElement(By.Id("Amount")));

        var amount = driver.FindElement(By.Id("Amount"));

        amount.Clear();
        amount.SendKeys("25.00");

        new SelectElement(
            driver.FindElement(By.Id("CurrencyCode")))
            .SelectByValue("ZAR");

        new SelectElement(
            driver.FindElement(By.Id("DonationType")))
            .SelectByValue("OnceOff");

        var projectSelect = new SelectElement(
            driver.FindElement(By.Id("ReliefProjectId")));

        var localProject = projectSelect.Options.FirstOrDefault(
            option => option.Text.Contains(
                "Local Test Relief Project",
                StringComparison.OrdinalIgnoreCase));

        if (localProject != null)
        {
            var projectValue = localProject.GetAttribute("value");

            if (!string.IsNullOrWhiteSpace(projectValue))
            {
                projectSelect.SelectByValue(projectValue);
            }
        }

        var anonymousCheckbox =
            driver.FindElement(By.Id("IsAnonymous"));

        if (!anonymousCheckbox.Selected)
        {
            anonymousCheckbox.Click();
        }

        Assert.True(
            anonymousCheckbox.Selected,
            "The anonymous donation option must be selected.");

        driver.FindElement(
            By.CssSelector("form button[type='submit']"))
            .Click();

        wait.Until(d =>
            d.Url.Contains(
                "/Donation/Confirmation/",
                StringComparison.OrdinalIgnoreCase));

        Assert.Contains(
            "/Donation/Confirmation/",
            driver.Url,
            StringComparison.OrdinalIgnoreCase);

        Assert.True(
            driver.PageSource.Contains(
                "Thank you for your support",
                StringComparison.OrdinalIgnoreCase),
            "The donation confirmation message was not displayed.");

        SaveEvidence(
            driver,
            "Anonymous_Donation_Confirmation");
    }

    [Fact]
    public void EmployeeLogin_OpensEmployeeDashboard()
    {
        var email =
            Environment.GetEnvironmentVariable(
                "GOTG_TEST_EMPLOYEE_EMAIL");

        var password =
            Environment.GetEnvironmentVariable(
                "GOTG_TEST_EMPLOYEE_PASSWORD");

        Assert.False(
            string.IsNullOrWhiteSpace(email),
            "Set GOTG_TEST_EMPLOYEE_EMAIL before running the Selenium tests.");

        Assert.False(
            string.IsNullOrWhiteSpace(password),
            "Set GOTG_TEST_EMPLOYEE_PASSWORD before running the Selenium tests.");

        using var driver = CreateDriver();
        var wait = CreateWait(driver);

        driver.Navigate().GoToUrl(
            $"{BaseUrl}/Account/Login");

        wait.Until(
            d => d.FindElement(By.Id("Email")));

        driver.FindElement(By.Id("Email"))
            .SendKeys(email!);

        driver.FindElement(By.Id("Password"))
            .SendKeys(password!);

        driver.FindElement(
            By.CssSelector("form button[type='submit']"))
            .Click();

        var dashboardHeading = wait.Until(d =>
            d.FindElements(By.TagName("h1"))
                .FirstOrDefault(element =>
                    element.Displayed &&
                    element.Text.Contains(
                        "Employee Dashboard",
                        StringComparison.OrdinalIgnoreCase)));

        Assert.NotNull(dashboardHeading);

        Assert.Contains(
            "/Employee",
            driver.Url,
            StringComparison.OrdinalIgnoreCase);

        SaveEvidence(
            driver,
            "Employee_Dashboard_Login");
    }
}