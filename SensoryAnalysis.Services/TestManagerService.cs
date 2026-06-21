using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services.Helpers;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading.Tasks;

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
        _logger.LogInformation($"A test add request has reached TestManagerService");
        _logger.LogDebug("Test add request data:\n" +
            $"Is null: {request is null}\n" +
            $"Name: {request?.Name}\n" +
            $"Type: {request?.TestType}\n" +
            $"Significance: {request?.Significance}\n" +
            $"Sample 1: {request?.NameOfSample1}\n" +
            $"Sample 2: {request?.NameOfSample2}");

        ArgumentNullException.ThrowIfNull(request);
        ValidatorHelper.ValidateObject(request);
        Test test = request.ToTest();
        await _db.AddTestAsync(test);
        return TestToTestResponse(test);
    }

    public async Task<TestResponse> AddJudgerToTestAsync(Guid testId)
    {
        _logger.LogInformation($"A request to add a judger to the {testId} test " +
            $"has reached TestManagerService");

        Test? test = await _db.GetTestByIdAsync(testId);
        if (test is null)
        {
            throw new ArgumentException("No matching Id");
        }
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

        ITestService service = _serviceFactory.GetTestService(test.TestType);
        judge.Samples = service.GenerateSamples(differentSample: lessFrequentType);
        await _db.AddJudgerToTestAsync(judge, test.Id);
        return TestToTestResponse(test);
    }

    #endregion

    #region Reading 

    public async Task<TestResponse?> GetTestByIdAsync(Guid id)
    {
        _logger.LogInformation($"A request to get the {id} test has reached TestManagerService");

        Test? test = await _db.GetTestByIdAsync(id);
        if (test is null) return null;
        return TestToTestResponse(test);
    }

    public async Task<List<TestResponse>> GetAllTestsAsync()
    {
        return (await _db.GetAllTestsAsync()).Select(TestToTestResponse).ToList();
    }

    public async Task<List<JudgerResponse>> GetJudgersFromTestAsync(Guid testId)
    {
        _logger.LogInformation($"A request to get the judgers from {testId} " +
            $"has reached TestManagerService");

        Test? test = await _db.GetTestByIdAsync(testId);
        if (test is null)
        {
            throw new ArgumentException("No matching Id");
        }
        return test.Judgers.Select(j => j.ToJudgerResponse()).ToList();
    }

    public async Task<List<string>> GetSamplesInfoAsync(Guid testId)
    {
        _logger.LogInformation($"A request to get the samples from the {testId} " +
            $"test has reached TestManagerService");

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

    public async Task<TestResponse> AddAnswerToTestAsync(Guid testId, Guid judgerId, Guid? chosenSample)
    {
        _logger.LogInformation($"A request to add an answer in the {testId} test " +
            $"has reached TestManagerService");
        _logger.LogDebug("AddAnswerToTest information:\n" +
            $"testId: {testId}\n" +
            $"judgerId: {judgerId}\n" +
            $"chosenSample: {(chosenSample?.ToString() ?? "null")}");

        Test? test = await _db.GetTestByIdAsync(testId);
        if (test is null)
        {
            throw new ArgumentException("Invalid test id");
        }
        await _db.AddAnswerToTestAsync(judgerId, chosenSample);
        return TestToTestResponse(test);
    }

    public async Task<TestResponse> AddAnswerToTestAsync(Guid testId, Guid judgerId, int? chosenSample)
    {
        _logger.LogInformation($"A request to add an answer in the {testId} test " +
            $"has reached TestManagerService");
        _logger.LogDebug("AddAnswerToTest information:\n" +
            $"testId: {testId}\n" +
            $"judgerId: {judgerId}\n" +
            $"chosenSample: {(chosenSample?.ToString() ?? "null")}");

        Test? test = await _db.GetTestByIdAsync(testId);
        if (test is null)
        {
            throw new ArgumentException("Invalid test id");
        }
        await _db.AddAnswerToTestAsync(judgerId, chosenSample);
        return TestToTestResponse(test);
    }

    public async Task<TestResult> GetTestResultsAsync(Guid testId)
    {
        _logger.LogInformation($"A request to get the {testId} test's results " +
            $"has reached TestManagerService.");

        Test? test = await _db.GetTestByIdAsync(testId);
        if (test is null)
        {
            throw new ArgumentException("Invalid test Id");
        }
        ITestService service = _serviceFactory.GetTestService(test.TestType);
        return service.GetTestResult(test);
    }

    #endregion

    #region Deleting

    public async Task<bool> DeleteTestAsync(Guid testId)
    {
        _logger.LogInformation($"A request to delete the {testId} test has reached " +
            $"TestManagerService.");

        bool sucess = await _db.DeleteTestAsync(testId);
        if (!sucess)
        {
            _logger.LogWarning($"Deletion of the {testId} test has failed");
        }
        return sucess;
    }

    public async Task<bool> RemoveJudgerFromTestAsync(Guid testId, Guid judgerId)
    {
        _logger.LogInformation($"A request to remove the {judgerId} judger from " +
            $"the {testId} test has reached TestManagerService.");

        bool sucess = await _db.RemoveJudgerFromTestAsync(judgerId);
        if (!sucess)
        {
            _logger.LogWarning($"Deletion of the {judgerId} judger from the " +
                $"{testId} test has failed");
        }
        return sucess;
    }

    #endregion

    #region Helpers

    private TestResponse TestToTestResponse(Test test)
    {
        ITestService service = _serviceFactory.GetTestService(test.TestType);
        return service.GetTestResponse(test);
    }

    #endregion
}