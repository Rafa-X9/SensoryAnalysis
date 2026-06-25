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
    private readonly ILogger<TestController> _logger;

    public TestController(ITestManagerService testManager,
        ITestServiceFactory serviceFactory,
        ILogger<TestController> logger)
    {
        _testManager = testManager;
        _serviceFactory = serviceFactory;
        _logger = logger;
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
        _logger.LogInformation("A test add request has been made.\n" +
            "        Name: {RequestName}\n" +
            "        Type: {RequestTestType}\n" +
            "        Significance: {RequestSignificance}\n" +
            "        Sample 1: {RequestNameOfSample1}\n" +
            "        Sample 2: {RequestNameOfSample2}",

            request.Name,
            request.TestType,
            request.Significance,
            request.NameOfSample1,
            request.NameOfSample2);

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
        _logger.LogInformation("A test deletion for id {TestId} request was made", id);

        await _testManager.DeleteTestAsync(id);
        return RedirectToAction("Index");
    }

    [Route("{id:guid}")]
    public async Task<IActionResult> ViewTest(Guid id)
    {
        TestResponse? test;
        test = await _testManager.GetTestResultsAsync(id);
        if (test is null) return RedirectToAction("Index");
        ViewBag.Result = test.Result;
        return View(test);
    }

    [Route("addjudger")]
    public async Task<IActionResult> AddJudger(Guid id, int amount)
    {
        _logger.LogInformation("A request to add {JudgerAmount} judgers to test {TestId} was made",
            amount, id);
        await _testManager.AddJudgersToTestAsync(id, amount);
        return RedirectToAction("ViewTest", new { id });
    }

    [Route("submitanswer")]
    [HttpPost]
    public async Task<IActionResult> SubmitAnswer(Guid testId, Guid judgerId, int answer)
    {
        _logger.LogInformation("A request to add the {Answer} answer to the " +
            "{JudgerId} judger in the {TestId} test was made",
            answer, judgerId, testId);

        TestResponse? test = await _testManager.GetTestByIdAsync(testId);
        if (test is null) return RedirectToAction("Index");
        if (test.Judgers.Any(j => j.Id == judgerId &&
            (answer == -1 || j.Samples.Any(s => s.Number == answer))))
        {
            await _testManager.AddAnswerToTestAsync(testId, judgerId, (answer == -1) ? null : answer);
        }
        ViewBag.Result = (await _testManager.GetTestResultsAsync(test.Id))?.Result;
        return PartialView("ResultsTablePartial", test);
    }

    [Route("removejudger")]
    [HttpGet]
    public async Task<IActionResult> RemoveJudgerFromTest(Guid testId, Guid judgerId)
    {
        _logger.LogInformation("A request to remove the {JudgerId} judger from the {TestId} " +
            "test was made", judgerId, testId);

        await _testManager.RemoveJudgerFromTestAsync(testId, judgerId);
        return RedirectToAction("ViewTest", new { id = testId });
    }

    [Route("testrecordpdf")]
    public async Task<IActionResult> TestRecordPDF(Guid id)
    {
        _logger.LogInformation("A request to get the {TestId} test records' PDF was made", id);

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
        _logger.LogInformation("A request to get the {TestId} test's sample paper sheet PDF was made", id);

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