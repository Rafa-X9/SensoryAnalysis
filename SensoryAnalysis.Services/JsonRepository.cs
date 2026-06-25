using Microsoft.Extensions.Configuration;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Entities;
using System.Text.Json;

namespace SensoryAnalysis.Services;
public class JsonRepository : ITestRepository
{
    private readonly string _dbPath = string.Empty;
    private List<Test> _tests;

    public JsonRepository(IConfiguration configuration)
    {
        string? path = configuration["DbFilePath"];
        if (path is null)
        {
            throw new InvalidOperationException("No DbFilePath registered");
        }
        _dbPath = path;
        using StreamReader sr = new(path);
        _tests = [];
    }

    public async Task<List<Test>> GetAllTestsAsync()
    {
        await PullTests();
        return _tests;
    }

    public async Task<Test?> GetTestByIdAsync(Guid id, bool includeJudgers = true)
    {
        await PullTests();
        return _tests.FirstOrDefault(temp => temp.Id == id);
    }

    public async Task<Test> AddTestAsync(Test test)
    {
        await PullTests();
        _tests.Add(test);
        await PushTests();
        return test;
    }

    public async Task<Test> AddJudgerToTestAsync(Judger judger, Guid testId)
    {
        await PullTests();
        Test test = _tests.First(temp => temp.Id == testId);
        test.Judgers.Add(judger);
        await PushTests();
        return test;
    }

    public async Task<bool> DeleteTestAsync(Guid testId)
    {
        await PullTests();
        int before = _tests.Count;
        _tests.RemoveAll(temp => temp.Id == testId);
        int after = _tests.Count;
        await PushTests();
        return before > after;
    }

    public async Task<bool> RemoveJudgerFromTestAsync(Guid judgerId)
    {
        await PullTests();
        foreach (Test test in _tests)
        {
            int before = test.Judgers.Count;
            test.Judgers.RemoveAll(temp => temp.Id == judgerId);
            int after = test.Judgers.Count;
            if (before > after)
            {
                await PushTests();
                return true;
            }
        }
        return false;
    }

    public async Task<Test> AddAnswerToTestAsync(Guid judgerId, Guid? chosenSample)
    {
        await PullTests();
        Test test = _tests.First(temp => temp.Judgers.Any(judger => judger.Id == judgerId));
        Judger judger = test.Judgers.First(temp => temp.Id == judgerId);
        if (chosenSample is null)
        {
            judger.Answer = null;
        }
        else
        {
            judger.Answer = judger.Samples.First(s => s.Id == chosenSample.Value).Number;
        }
        await PushTests();
        return test;
    }

    public async Task<Test> AddAnswerToTestAsync(Guid judgerId, int? chosenSample)
    {
        await PullTests();
        Test test = _tests.First(temp => temp.Judgers.Any(judger => judger.Id == judgerId));
        Judger judger = test.Judgers.First(temp => temp.Id == judgerId);
        judger.Answer = chosenSample;
        await PushTests();
        return test;
    }

    public Task AddJudgersAsync(Test test)
    {
        return Task.CompletedTask;
    }
    private async Task PullTests()
    {
        using StreamReader sr = new(_dbPath);
        string? line = await sr.ReadLineAsync();
        if (line is null) return;
        var list = JsonSerializer.Deserialize<List<Test>>(line);
        if (list is null)
        {
            throw new InvalidOperationException("DbFilePath has invalid JSON");
        }
        _tests = list;
    }

    private async Task PushTests()
    {
        using StreamWriter sw = new(_dbPath, false);
        await sw.WriteAsync(JsonSerializer.Serialize(_tests));
    }
}