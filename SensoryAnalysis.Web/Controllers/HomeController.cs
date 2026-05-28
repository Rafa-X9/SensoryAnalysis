using Microsoft.AspNetCore.Mvc;

namespace SensoryAnalysis.Web.Controllers;
[Route("home")]
public class HomeController : Controller
{
    [Route("")]
    public IActionResult Index()
    {
        return View();
    }

    [Route("/")]
    public IActionResult RedirectToIndex()
    {
        return RedirectToAction("Index");
    }

    [Route("about")]
    public IActionResult About()
    {
        return View();
    }
}