using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services.Helpers;

namespace SensoryAnalysis.Services;
/// <summary>
/// The general tests service to store the tasts and do CRUD operations
/// </summary>
public class TestManagerService : ITestManagerService
{
    private readonly List<Test> _tests;
    private readonly ITestServiceFactory _serviceFactory;

    public TestManagerService(ITestServiceFactory serviceFactory)
    {
        _tests = [];
        _serviceFactory = serviceFactory;
    }

    #region Creating

    public TestResponse AddTest(TestAddRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatorHelper.ValidateObject(request);
        Test test = request.ToTest();
        _tests.Add(test);
        return test.ToTestResponse();
    }

    public TestResponse AddJudgerToTest(Guid testId)
    {
        Test? test = _tests.FirstOrDefault(t => t.Id == testId);
        if (test is null)
        {
            throw new ArgumentException("No matching Id");
        }
        Judger judge = new(test.Id, []);

        SampleTypes lessFrequentType;
        if (test.Judgers.Count(j => j.Samples.Count(s => s.SampleType == SampleTypes.Sample1) == 1) < test.Judgers.Count / 2)
        {
            lessFrequentType = SampleTypes.Sample1;
        }
        else
        {
            lessFrequentType = SampleTypes.Sample2;
        }

        ITestService service = _serviceFactory.GetTestService(test.TestType);
        judge.Samples = service.GenerateSamples(differentSample: lessFrequentType);

        test.Judgers.Add(judge);
        return test.ToTestResponse();
    }

    #endregion

    #region Reading 

    public TestResponse? GetTestById(Guid id)
    {
        return _tests.FirstOrDefault(t => t.Id == id)?.ToTestResponse();
    }

    public List<TestResponse> GetAllTests()
    {
        return _tests.Select(t => t.ToTestResponse()).ToList();
    }

    public List<JudgerResponse> GetJudgersFromTest(Guid testId)
    {
        Test? test = _tests.FirstOrDefault(t => t.Id == testId);
        if (test is null)
        {
            throw new ArgumentException("No matching Id");
        }
        return test.Judgers.Select(j => j.ToJudgerResponse()).ToList();
    }

    #endregion

    #region Making the test

    public TestResponse AddAnswerToTest(Guid testId, Guid judgerId, Guid? chosenSample)
    {
        Test? test = _tests.FirstOrDefault(t => t.Id == testId);
        if (test is null)
        {
            throw new ArgumentException("Invalid test id");
        }
        Judger? judger = test.Judgers.FirstOrDefault(j => j.Id == judgerId);
        if (judger is null)
        {
            throw new ArgumentException("Invalid judger id");
        }
        Sample? sample = judger.Samples.FirstOrDefault(s => s.Id == chosenSample);
        if (sample is null)
        {
            throw new ArgumentException("Invalid sample id");
        }
        judger.Answer = sample.Number;
        return test.ToTestResponse();
    }

    public TestResponse AddAnswerToTest(Guid testId, Guid judgerId, int? chosenSample)
    {
        Test? test = _tests.FirstOrDefault(t => t.Id == testId);
        if (test is null)
        {
            throw new ArgumentException("Invalid test id");
        }
        Judger? judger = test.Judgers.FirstOrDefault(j => j.Id == judgerId);
        if (judger is null)
        {
            throw new ArgumentException("Invalid judger id");
        }
        Sample? sample = judger.Samples.FirstOrDefault(s => s.Number == chosenSample);
        if (sample is null)
        {
            throw new ArgumentException("Invalid sample number");
        }
        judger.Answer = sample.Number;
        return test.ToTestResponse();
    }

    public TestResult GetTestResults(Guid testId)
    {
        Test? test = _tests.FirstOrDefault(t => t.Id == testId);
        if (test is null)
        {
            throw new ArgumentException("Invalid test Id");
        }
        ITestService service = _serviceFactory.GetTestService(test.TestType);
        return service.GetTestResult(test);
    }

    #endregion
}