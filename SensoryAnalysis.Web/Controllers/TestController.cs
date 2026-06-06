using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
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

    [Route("{id:guid}")]
    public IActionResult ViewTest(Guid id)
    {
        TestResponse? test = _testManager.GetTestById(id);
        if (test is null) return RedirectToAction("Index");
        ViewBag.Result = _testManager.GetTestResults(id);
        return View(test);
    }

    [Route("addjudger")]
    public IActionResult AddJudger(Guid id, int amount)
    {
        if (amount <= 0 || _testManager.GetTestById(id) is null)
        {
            return RedirectToAction("Index");
        }
        for (int i = 0; i < amount; i++)
        {
            _testManager.AddJudgerToTest(id);
        }
        return RedirectToAction("ViewTest", new { id });
    }

    [Route("submitanswer")]
    [HttpPost]
    public IActionResult SubmitAnswer(Guid testId, Guid judgerId, int answer)
    {
        TestResponse? test = _testManager.GetTestById(testId);
        if (test is null) return RedirectToAction("Index");
        if (test.Judgers.Any(j => j.Id == judgerId &&
            (answer == -1 || j.Samples.Any(s => s.Number == answer))))
        {
            _testManager.AddAnswerToTest(testId, judgerId, (answer == -1) ? null : answer);
        }
        ViewBag.Result = _testManager.GetTestResults(test.Id);
        return PartialView("ResultsTablePartial", test);
    }

    [Route("removejudger")]
    [HttpGet]
    public IActionResult RemoveJudgerFromTest(Guid testId, Guid judgerId)
    {
        _testManager.RemoveJudgerFromTest(testId, judgerId);
        return RedirectToAction("ViewTest", new { id = testId });
    }

    [Route("testrecordpdf")]
    public IActionResult TestRecordPDF(Guid id)
    {
        TestResponse? test = _testManager.GetTestById(id);
        if (test is null) return RedirectToAction("Index");

        ViewBag.Instructions = "Você está recebendo 3 amostras codificadas. " +
            "Duas amostras são iguais e uma diferente. Por favor, avalie " +
            "as amostras da esquerda para a direita e marque a amostra " +
            "diferente.";

        return new ViewAsPdf("TestRecordPDF", test, ViewData)
        {
            FileName = _nameToFileName(test.Name) + " - Fichas.pdf",
            PageSize = Rotativa.AspNetCore.Options.Size.A4,
            PageMargins = new(20, 20, 20, 20)
        };
        //return View(test);
    }

    [Route("SamplePaperSheetPDF")]
    public IActionResult SamplePaperSheetPDF(Guid id)
    {
        TestResponse? test = _testManager.GetTestById(id);
        if (test is null) return RedirectToAction("Index");
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