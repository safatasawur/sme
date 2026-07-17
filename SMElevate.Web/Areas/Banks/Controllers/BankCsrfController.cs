using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace SMElevate.Web.Areas.Banks.Controllers;

// Endpoint used by AJAX to retrieve the anti-forgery token
[Area("Banks")]
public class BankCsrfController : Controller
{
    private readonly IAntiforgery _antiforgery;
    public BankCsrfController(IAntiforgery antiforgery) => _antiforgery = antiforgery;

    [HttpGet]
    public IActionResult GetToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return Json(new { token = tokens.RequestToken });
    }
}
