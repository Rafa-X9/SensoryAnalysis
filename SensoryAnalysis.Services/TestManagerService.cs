using Microsoft.Extensions.Configuration;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services.Helpers;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;

namespace SensoryAnalysis.Services;
/// <summary>
/// The general tests service to store the tasts and do CRUD operations
/// </summary>
public class TestManagerService : ITestManagerService
{
    private readonly List<Test> _tests;
    private readonly ITestServiceFactory _serviceFactory;
    private readonly bool _useDatabase;
    private readonly string _dbPath = string.Empty;

    public TestManagerService(ITestServiceFactory serviceFactory,
        IConfiguration? configuration,
        bool useDatabase = true)
    {
        _tests = [];
        _serviceFactory = serviceFactory;
        _useDatabase = useDatabase;

        if (configuration is not null && useDatabase)
        {
            string? path = configuration["DbFilePath"];
            if (path is null)
            {
                throw new InvalidOperationException("No DbFilePath registered");
            }
            _dbPath = path;
            using StreamReader sr = new(path);
            string? line = sr.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                var list = JsonSerializer.Deserialize<List<Test>>(line);
                if (list is null)
                {
                    throw new InvalidOperationException("DbFilePath has invalid JSON");
                }
                _tests = list;
            }
        }
        else
        {
            _tests.Add(new("Teste de pão de queijo",
                TestTypes.Triangular,
                Significances._5,
                nameOfSample1: "Polvilho misto",
                nameOfSample2: "Polvilho azedo"));
        }
    }

    #region Creating

    public TestResponse AddTest(TestAddRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidatorHelper.ValidateObject(request);
        Test test = request.ToTest();
        _tests.Add(test);
        Save();
        return TestToTestResponse(test);
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

        Save();
        test.Judgers.Add(judge);
        return TestToTestResponse(test);
    }

    #endregion

    #region Reading 

    public TestResponse? GetTestById(Guid id)
    {
        Test? test = _tests.FirstOrDefault(t => t.Id == id);
        if (test is null) return null;
        return TestToTestResponse(test);
    }

    public List<TestResponse> GetAllTests()
    {
        return _tests.Select(TestToTestResponse).ToList();
    }

    public List<JudgerResponse> GetJudgersFromTest(Guid testId)
    {
        Test? test = _tests.FirstOrDefault(t => t.Id == testId);
        if (test is null)
        {
            throw new ArgumentException("No matching Id");
        }
        return TestToTestResponse(test).Judgers.ToList();
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
        Save();
        return TestToTestResponse(test);
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

        if (chosenSample is null)
        {
            judger.Answer = null;
            return test.ToTestResponse();
        }
        
        Sample? sample = judger.Samples.FirstOrDefault(s => s.Number == chosenSample);
        if (sample is null)
        {
            throw new ArgumentException("Invalid sample number");
        }
        judger.Answer = sample.Number;
        Save();
        return TestToTestResponse(test);
    }

    public TestResult GetTestResults(Guid testId)
    {
        Test? test = _tests.FirstOrDefault(t => t.Id == testId);
        if (test is null)
        {
            throw new ArgumentException("Invalid test Id");
        }
        ITestService service = _serviceFactory.GetTestService(test.TestType);
        Save();
        return service.GetTestResult(test);
    }

    #endregion

    #region Deleting

    public bool DeleteTest(Guid testId)
    {
        int before = _tests.Count;
        _tests.RemoveAll(test => test.Id == testId);
        int after = _tests.Count;
        Save();
        return before > after;
    }

    public bool RemoveJudgerFromTest(Guid testId, Guid judgerId)
    {
        Test? test = _tests.FirstOrDefault(t => t.Id == testId);
        if (test is null) return false;
        Judger? judger = test.Judgers.FirstOrDefault(j => j.Id == judgerId);
        if (judger is null) return false;
        test.Judgers.Remove(judger);
        return true;
    }

    #endregion

    #region Saving

    private void Save()
    {
        if (!_useDatabase || _dbPath == string.Empty) return;
        using StreamWriter sw = new(_dbPath, false);
        sw.Write(JsonSerializer.Serialize(_tests));
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