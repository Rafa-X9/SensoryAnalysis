using Microsoft.Extensions.Logging;
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
    private readonly ITestRepository _db;
    private readonly ITestServiceFactory _serviceFactory;
    private readonly ILogger<TestManagerService> _logger;

    public TestManagerService(ITestRepository db,
        ITestServiceFactory serviceFactory,
        ILogger<TestManagerService> logger)
    {
        _db = db;
        _serviceFactory = serviceFactory;
        _logger = logger;
    }

    #region Creating

    public async Task<TestResponse> AddTestAsync(TestAddRequest? request)
    {
        _logger.LogInformation("A test add request has reached {ServiceType}" +
            "Is null: {RequestIsNull}\n" +
            "Name: {RequestName}\n" +
            "Type: {RequestTestType}\n" +
            "Significance: {RequestSignificance}\n" +
            "Sample 1: {RequestNameOfSample1}\n" +
            "Sample 2: {RequestNameOfSample2}",

            nameof(TestManagerService),
            request is null,
            request?.Name,
            request?.TestType,
            request?.Significance,
            request?.NameOfSample1,
            request?.NameOfSample2);

        ArgumentNullException.ThrowIfNull(request);
        ValidatorHelper.ValidateObject(request);
        Test test = request.ToTest();
        await _db.AddTestAsync(test);
        return TestToTestResponse(test);
    }

    public Test AddJudgerToTest(Test test, ITestService testService)
    {
        _logger.LogInformation("A request to add a judger to the {TestId} test " +
            "has reached {ServiceType}",
            test.Id,
            nameof(TestManagerService));

        Judger judge = new(test.Id, []);
        SampleTypes lessFrequentType;
        if (test.Judgers.Count(j => j.Samples.Count(s => s.SampleType == SampleTypes.Sample1) == 1) <= test.Judgers.Count / 2)
        {
            lessFrequentType = SampleTypes.Sample1;
        }
        else
        {
            lessFrequentType = SampleTypes.Sample2;
        }
        judge.Samples = testService.GenerateSamples(differentSample: lessFrequentType);
        test.Judgers.Add(judge);
        return test;
    }

    public async Task<TestResponse> AddJudgersToTestAsync(Guid testId, int amount)
    {
        Test? test = await _db.GetTestByIdAsync(testId, includeJudgers: false);
        if (test is null) throw new Exception();
        var service = _serviceFactory.GetTestService(test.TestType);
        for (int i = 0; i < amount; i++)
        {
            test = AddJudgerToTest(test, service);
        }
        if (test is null)
        {
            throw new ArgumentException("Invalid amount");
        }
        await _db.AddJudgersAsync(test);
        TestResponse response = GetTestResults(test);
        return response;
    }

    #endregion

    #region Reading 

    public async Task<TestResponse?> GetTestByIdAsync(Guid id)
    {
        _logger.LogInformation("A request to get the {TestId} test has reached " +
            $"TestManagerService", id);

        Test? test = await _db.GetTestByIdAsync(id);
        if (test is null) return null;
        return TestToTestResponse(test);
    }

    public async Task<List<TestResponse>> GetAllTestsAsync()
    {
        return (await _db.GetAllTestsAsync()).Select(temp => TestToTestResponse(temp)).ToList();
    }
    
    public async Task<List<string>> GetSamplesInfoAsync(Guid testId)
    {
        _logger.LogInformation("A request to get the samples from the {TestId} " +
            "test has reached {ServiceType}",
            testId, nameof(TestManagerService));

        Test? test = await _db.GetTestByIdAsync(testId);
        if (test is null)
        {
            throw new ArgumentException("No matching Id");
        }
        ITestService service = _serviceFactory.GetTestService(test.TestType);
        List<string> info = [];
        foreach (Judger judger in test.Judgers)
        {
            info.Add(service.SamplesInfo(judger, test));
        }
        return info;
    }

    #endregion

    #region Making the test

    public async Task<TestResponse> AddAnswerToTestAsync(Guid testId, Guid judgerId, int? chosenSample)
    {
        _logger.LogInformation("A request to add an answer in the {testId} test " +
            "has reached {ServiceType}" +
            "judgerId: {JudgerId}\n" +
            "chosenSample: {Answer}",

            testId,
            typeof(TestManagerService),
            judgerId,
            (chosenSample?.ToString() ?? "null"));

        Test? test = await _db.GetTestByIdAsync(testId);
        if (test is null)
        {
            throw new ArgumentException("Invalid test id");
        }
        await _db.AddAnswerToTestAsync(judgerId, chosenSample);
        return TestToTestResponse(test);
    }

    public TestResponse GetTestResults(Test test)
    {
        _logger.LogInformation("A request to get the {TestId} test's results " +
            $"has reached TestManagerService.", test.Id);
        ITestService service = _serviceFactory.GetTestService(test.TestType);
        TestResult result = service.GetTestResult(test);
        return test.ToTestResponse(result);
    }

    public async Task<TestResponse?> GetTestResultsAsync(Guid testId)
    {
        Test? test = await _db.GetTestByIdAsync(testId);
        if (test is null) return null;
        return GetTestResults(test);
    }

    #endregion

    #region Deleting

    public async Task<bool> DeleteTestAsync(Guid testId)
    {
        _logger.LogInformation("A request to delete the {TestId} test has reached " +
            $"TestManagerService.", testId);

        bool sucess = await _db.DeleteTestAsync(testId);
        if (!sucess)
        {
            _logger.LogWarning("Deletion of the {TestId} test has failed", testId);
        }
        return sucess;
    }

    public async Task<bool> RemoveJudgerFromTestAsync(Guid testId, Guid judgerId)
    {
        _logger.LogInformation("A request to remove the {JudgerId} judger from " +
            "the {TestId} test has reached TestManagerService.",
            judgerId, testId);

        bool sucess = await _db.RemoveJudgerFromTestAsync(judgerId);
        if (!sucess)
        {
            _logger.LogWarning("Deletion of the {JudgerId} judger from the " +
                "{TestId} test has failed",
                judgerId, testId);
        }
        return sucess;
    }

    #endregion

    #region Helpers

    private TestResponse TestToTestResponse(Test test, TestResult? result = null)
    {
        ITestService service = _serviceFactory.GetTestService(test.TestType);
        TestResponse response = service.GetTestResponse(test);
        response.Result = result;
        return response;
    }

    #endregion
}