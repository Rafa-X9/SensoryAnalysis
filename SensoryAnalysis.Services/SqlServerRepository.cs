using Microsoft.EntityFrameworkCore;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Services;
public class SqlServerRepository : ITestRepository
{
    private readonly ApplicationDbContext _db;

    public SqlServerRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<Test>> GetAllTestsAsync()
    {
        var list = await _db.Tests.AsNoTracking().ToListAsync();
        return list;
    }

    public async Task<Test?> GetTestByIdAsync(Guid id, bool includeJudgers = true)
    {
        if (includeJudgers)
        {
            return await _db.Tests
                .Include(temp => temp.Judgers.OrderBy(judger => judger.CreatedAt))
                .ThenInclude(temp => temp.Samples.OrderBy(sample => sample.CreatedAt))
                .AsNoTracking()
                .FirstOrDefaultAsync(temp => temp.Id == id);
        }
        else
        {
            return await _db.Tests
                .AsNoTracking()
                .FirstOrDefaultAsync(temp => temp.Id == id);
        }
    }

    public async Task<Test> AddTestAsync(Test test)
    {
        _db.Tests.Add(test);
        await _db.SaveChangesAsync();
        return test;
    }

    public async Task<Test> AddJudgerToTestAsync(Judger judger, Guid testId)
    {
        Test test = await _db.Tests.FirstAsync(temp => temp.Id == testId);
        Judger toAdd = new()
        {
            Id = judger.Id,
            TestId = testId,
            Samples = judger.Samples
        };
        test.Judgers.Add(toAdd);
        _db.Judgers.Add(toAdd);
        await _db.SaveChangesAsync();
        return test;
    }

    public async Task<Test> AddAnswerToTestAsync(Guid judgerId, Guid? chosenSample)
    {
        if (chosenSample is null)
        {
            _db.Judgers.First(temp => temp.Id == judgerId).Answer = null;
        }
        else
        {
            _db.Judgers.First(temp => temp.Id == judgerId)
                .Answer = _db.Samples.First(temp => temp.Id == chosenSample).Number;
        }
        await _db.SaveChangesAsync();
        return _db.Tests.First(temp => temp.Judgers.Any(judger => judger.Id == judgerId));
    }

    public async Task<Test> AddAnswerToTestAsync(Guid judgerId, int? chosenSample)
    {
        if (chosenSample is null)
        {
            _db.Judgers.First(temp => temp.Id == judgerId).Answer = null;
        }
        else
        {
            _db.Judgers.First(temp => temp.Id == judgerId)
                .Answer = _db.Samples.First(temp => temp.Number == chosenSample).Number;
        }
        await _db.SaveChangesAsync();
        return _db.Tests.First(temp => temp.Judgers.Any(judger => judger.Id == judgerId));
    }

    public async Task<bool> DeleteTestAsync(Guid testId)
    {
        return (await _db.Tests.Where(temp => temp.Id == testId)
            .ExecuteDeleteAsync()) > 0;
    }

    public async Task<bool> RemoveJudgerFromTestAsync(Guid judgerId)
    {
        Judger judger = await _db.Judgers.FirstAsync(temp => temp.Id == judgerId);
        Test test = await _db.Tests.FirstAsync(temp => temp.Id == judger.TestId);
        test.Judgers.Remove(judger);
        return (await _db.SaveChangesAsync()) > 0;
    }

    public async Task AddJudgersAsync(Test test)
    {
        _db.Judgers.AddRange(test.Judgers);
        await _db.SaveChangesAsync();
    }
}