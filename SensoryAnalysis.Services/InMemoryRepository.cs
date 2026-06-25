using SensoryAnalysis.Contracts;
using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Services;
public class InMemoryRepository : ITestRepository
{
    private readonly List<Test> _tests = [];

    public Task<List<Test>> GetAllTestsAsync()
    {
        return Task.FromResult(_tests);
    }

    public Task<Test?> GetTestByIdAsync(Guid id, bool includeJudgers = true)
    {
        return Task.FromResult(_tests.FirstOrDefault(temp => temp.Id == id));
    }

    public Task<Test> AddTestAsync(Test test)
    {
        _tests.Add(test);
        return Task.FromResult(test);
    }

    public Task<Test> AddJudgerToTestAsync(Judger judger, Guid testId)
    {
        Test test = _tests.First(temp => temp.Id == testId);
        test.Judgers.Add(judger);
        return Task.FromResult(test);
    }

    public Task<bool> DeleteTestAsync(Guid testId)
    {
        int before = _tests.Count;
        _tests.RemoveAll(temp => temp.Id == testId);
        int after = _tests.Count;
        return Task.FromResult(before > after);
    }

    public Task<bool> RemoveJudgerFromTestAsync(Guid judgerId)
    {
        foreach (Test test in _tests)
        {
            int before = test.Judgers.Count;
            test.Judgers.RemoveAll(temp => temp.Id == judgerId);
            int after = test.Judgers.Count;
            if (before > after) return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<Test> AddAnswerToTestAsync(Guid judgerId, Guid? chosenSample)
    {
        Test test = _tests.First(temp => temp.Judgers.Any(judger => judger.Id == judgerId));
        Judger judger = test.Judgers.First(temp => temp.Id == judgerId);
        if (chosenSample is null)
        {
            judger.Answer = null;
        }
        else
        {
            judger.Answer = judger.Samples.First(temp => temp.Id == chosenSample).Number;
        }
        return Task.FromResult(test);
    }

    public Task<Test> AddAnswerToTestAsync(Guid judgerId, int? chosenSample)
    {
        Test test = _tests.First(temp => temp.Judgers.Any(judger => judger.Id == judgerId));
        Judger judger = test.Judgers.First(temp => temp.Id == judgerId);
        if (chosenSample is null)
        {
            judger.Answer = null;
        }
        else
        {
            judger.Answer = judger.Samples.First(temp => temp.Number == chosenSample).Number;
        }
        return Task.FromResult(test);
    }

    public Task AddJudgersAsync(Test test)
    {
        return Task.CompletedTask;
    }
}