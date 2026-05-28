using Microsoft.AspNetCore.Mvc;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Web.Controllers;

[Route("/tests")]
public class TestController : Controller
{
    private readonly ITestManagerService _testManager;

    public TestController(ITestManagerService testManager)
    {
        _testManager = testManager;
    }

    [Route("")]
    public IActionResult Index()
    {
        return View(_testManager.GetAllTests());
    }

    [Route("create")]
    [HttpGet]
    public IActionResult Create()
    {
        List<(string name, TestTypes type)> testTypes = new()
        {
            ("Triangular", TestTypes.Triangular)
        };
        ViewBag.TestTypes = testTypes;

        List<(string name, Significances significance)> significances =
        [
            ("0.1%", Significances._01),
            ("1%", Significances._1),
            ("5%", Significances._5),
            ("10%", Significances._10),
            ("20%", Significances._20),
        ];
        ViewBag.Significances = significances;

        return View(new TestAddRequest());
    }

    [Route("create")]
    [HttpPost]
    public IActionResult Create(TestAddRequest request)
    {
        if (ModelState.IsValid)
        {
            _testManager.AddTest(request);
        }
        return RedirectToAction("Index");
    }

    [Route("delete")]
    [HttpGet]
    public IActionResult Delete(Guid id)
    {
        TestResponse? test = _testManager.GetTestById(id);
        if (test is null) return RedirectToAction("Index");
        return View(test);
    }

    [Route("delete")]
    [HttpPost]
    public IActionResult Delete(Guid id, bool soggyCatIsPeak = true)
    {
        _testManager.DeleteTest(id);
        return RedirectToAction("Index");
    }
}