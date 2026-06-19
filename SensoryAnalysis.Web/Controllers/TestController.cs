using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;
using System.Threading.Tasks;

namespace SensoryAnalysis.Web.Controllers;

[Route("/tests")]
public class TestController : Controller
{
    private readonly ITestManagerService _testManager;
    private readonly ITestServiceFactory _serviceFactory;

    public TestController(ITestManagerService testManager, ITestServiceFactory serviceFactory)
    {
        _testManager = testManager;
        _serviceFactory = serviceFactory;
    }

    [Route("")]
    public async Task<IActionResult> Index()
    {
        return View(await _testManager.GetAllTestsAsync());
    }

    [Route("create")]
    [HttpGet]
    public IActionResult Create()
    {
        List<(string name, TestTypes type)> testTypes = new()
        {
            ("Triangular", TestTypes.Triangular),
            ("Duo-Trio", TestTypes.DuoTrio)
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
    public async Task<IActionResult> Create(TestAddRequest request)
    {
        if (ModelState.IsValid)
        {
            await _testManager.AddTestAsync(request);
        }
        return RedirectToAction("Index");
    }

    [Route("delete")]
    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        TestResponse? test = await _testManager.GetTestByIdAsync(id);
        if (test is null) return RedirectToAction("Index");
        return View(test);
    }

    [Route("delete")]
    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, bool soggyCatIsPeak = true)
    {
        await _testManager.DeleteTestAsync(id);
        return RedirectToAction("Index");
    }

    [Route("{id:guid}")]
    public async Task<IActionResult> ViewTest(Guid id)
    {
        TestResponse? test = await _testManager.GetTestByIdAsync(id);
        if (test is null) return RedirectToAction("Index");
        ViewBag.Result = await _testManager.GetTestResultsAsync(id);
        return View(test);
    }

    [Route("addjudger")]
    public async Task<IActionResult> AddJudger(Guid id, int amount)
    {
        if (amount <= 0 || await _testManager.GetTestByIdAsync(id) is null)
        {
            return RedirectToAction("Index");
        }
        for (int i = 0; i < amount; i++)
        {
            await _testManager.AddJudgerToTestAsync(id);
        }
        return RedirectToAction("ViewTest", new { id });
    }

    [Route("submitanswer")]
    [HttpPost]
    public async Task<IActionResult> SubmitAnswer(Guid testId, Guid judgerId, int answer)
    {
        TestResponse? test = await _testManager.GetTestByIdAsync(testId);
        if (test is null) return RedirectToAction("Index");
        if (test.Judgers.Any(j => j.Id == judgerId &&
            (answer == -1 || j.Samples.Any(s => s.Number == answer))))
        {
            await _testManager.AddAnswerToTestAsync(testId, judgerId, (answer == -1) ? null : answer);
        }
        ViewBag.Result = await _testManager.GetTestResultsAsync(test.Id);
        return PartialView("ResultsTablePartial", test);
    }

    [Route("removejudger")]
    [HttpGet]
    public async Task<IActionResult> RemoveJudgerFromTest(Guid testId, Guid judgerId)
    {
        await _testManager.RemoveJudgerFromTestAsync(testId, judgerId);
        return RedirectToAction("ViewTest", new { id = testId });
    }

    [Route("testrecordpdf")]
    public async Task<IActionResult> TestRecordPDF(Guid id)
    {
        TestResponse? test = await _testManager.GetTestByIdAsync(id);
        if (test is null) return RedirectToAction("Index");

        ITestService service = _serviceFactory.GetTestService(test.TestType);
        ViewBag.Instructions = service.Instructions();

        return new ViewAsPdf("TestRecordPDF", test, ViewData)
        {
            FileName = _nameToFileName(test.Name) + " - Fichas.pdf",
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageMargins = new(20, 20, 20, 20)
        };
        //return View(test);
    }

    [Route("SamplePaperSheetPDF")]
    public async Task<IActionResult> SamplePaperSheetPDF(Guid id)
    {
        TestResponse? test = await _testManager.GetTestByIdAsync(id);
        if (test is null) return RedirectToAction("Index");
        ViewBag.SamplesInfo = await _testManager.GetSamplesInfoAsync(id);

        return new ViewAsPdf("SamplePaperSheetPDF", test, ViewData)
        {
            FileName = _nameToFileName(test.Name) + " - Numeros.pdf",
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageMargins = new(20, 20, 20, 20)
        };
    }

    private static string _nameToFileName(string name)
    {
        name = name.Trim();
        name = name.Trim('.');
        name = name.Trim('-');
        name = name.Replace("<", "");
        name = name.Replace(">", "");
        name = name.Replace(":", "");
        name = name.Replace("\"", "");
        name = name.Replace("/", "");
        name = name.Replace("\\", "");
        name = name.Replace("|", "");
        name = name.Replace("?", "");
        name = name.Replace("*", "");
        name = name.Replace("ã", "a");
        return name;
    }
}