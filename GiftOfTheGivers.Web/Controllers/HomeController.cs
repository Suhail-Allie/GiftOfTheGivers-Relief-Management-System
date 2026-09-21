using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GiftOfTheGivers.Web.Models;

namespace GiftOfTheGivers.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult About()
{
    return View();
}

public IActionResult Contact()
{
    return View();
}

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
