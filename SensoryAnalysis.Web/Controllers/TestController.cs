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
        _logger.LogInformation("A test add request has been made.");
        
        _logger.LogDebug($"TestAddRequest object:\n" +
            $"        Name: {request.Name}\n" +
            $"        Type: {request.TestType}\n" +
            $"        Significance: {request.Significance}\n" +
            $"        Sample 1: {request.NameOfSample1}\n" +
            $"        Sample 2: {request.NameOfSample2}");

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
        _logger.LogInformation($"A test deletion for id {id} request was made");

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
        _logger.LogInformation($"A request to add {amount} judgers to test {id} was made");

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
        _logger.LogInformation($"A request to add the {answer} answer to the " +
            $"{judgerId} judger in the {testId} test was made");

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
        _logger.LogInformation($"A request to remove the {judgerId} judger from the {testId} test was made");

        await _testManager.RemoveJudgerFromTestAsync(testId, judgerId);
        return RedirectToAction("ViewTest", new { id = testId });
    }

    [Route("testrecordpdf")]
    public async Task<IActionResult> TestRecordPDF(Guid id)
    {
        _logger.LogInformation($"A request to get the {id} test records' PDF was made");

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
        _logger.LogInformation($"A request to get the {id} test's sample paper sheet PDF was made");

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