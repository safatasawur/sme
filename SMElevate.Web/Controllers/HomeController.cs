using Microsoft.AspNetCore.Mvc;

namespace SMElevate.Web.Controllers;

public class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login");

        var userType = User.FindFirst("UserType")?.Value;
        return userType switch
        {
            "Admin" => Redirect("/Admin"),
            "Bank" => Redirect("/Banks"),
            _ => Redirect("/EndUser")
        };
    }

    [HttpGet("/login")]
    public IActionResult Login() => View("PortalSelect");

    [HttpGet("/access-denied")]
    public IActionResult AccessDenied() => View();

    [HttpGet("/error")]
    public IActionResult Error() => View();
}
