using Microsoft.Extensions.Configuration;
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

    public TestManagerService(ITestRepository db, ITestServiceFactory serviceFactory)
    {
        _db = db;
        _serviceFactory = serviceFactory;
    }

    #region Creating

    public async Task<TestResponse> AddTestAsync(TestAddRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatorHelper.ValidateObject(request);
        Test test = request.ToTest();
        await _db.AddTestAsync(test);
        return TestToTestResponse(test);
    }

    public async Task<TestResponse> AddJudgerToTestAsync(Guid testId)
    {
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
        Test? test = await _db.GetTestByIdAsync(testId);
        if (test is null)
        {
            throw new ArgumentException("No matching Id");
        }
        return test.Judgers.Select(j => j.ToJudgerResponse()).ToList();
    }

    public async Task<List<string>> GetSamplesInfoAsync(Guid testId)
    {
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
        return await _db.DeleteTestAsync(testId);
    }

    public async Task<bool> RemoveJudgerFromTestAsync(Guid testId, Guid judgerId)
    {
        return await _db.RemoveJudgerFromTestAsync(judgerId);
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