using System.Diagnostics;
using ImageSharingWithModel.Models;
using Microsoft.AspNetCore.Mvc;

namespace ImageSharingWithModel.Controllers;

public class HomeController : BaseController
{
    [HttpGet]
    public IActionResult Index(string Username = "Stranger")
    {
        CheckAda();
        ViewBag.Title = "Welcome!";
        ViewBag.Username = Username;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(string ErrId)
    {
        CheckAda();
        return View(new ErrorViewModel
            { ErrId = ErrId, RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        CheckAda();
        return View();
    }
}