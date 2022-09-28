using Microsoft.AspNetCore.Mvc;

namespace ImageSharingWithModel.Controllers;

public class BaseController : Controller
{
    protected void CheckAda()
    {
        ViewBag.isADA = GetADAFlag();
    }

    protected bool GetADAFlag()
    {
        var cookie = Request.Cookies["ADA"];
        return cookie != null && "true".Equals(cookie);
    }

    protected string GetLoggedInUser()
    {
        return Request.Cookies["Username"];
    }

    protected ActionResult ForceLogin()
    {
        return RedirectToAction("Login", "Account");
    }
}