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
        return await _db.Tests.AsNoTracking().ToListAsync();
    }

    public async Task<Test?> GetTestByIdAsync(Guid id)
    {
        return await _db.Tests
            .Include(temp => temp.Judgers)
            .ThenInclude(temp => temp.Samples)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Test> AddTestAsync(Test test)
    {
        _db.Tests.Add(test);
        await _db.SaveChangesAsync();
        return test;
    }

    public async Task<Test> AddJudgerToTestAsync(Judger judger, Guid testId)
    {
        _db.Judgers.Add(new()
        {
            Id = judger.Id,
            TestId = testId,
            Samples = judger.Samples
        });
        await _db.SaveChangesAsync();
        return _db.Tests.First(temp => temp.Id == testId);
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
        _db.Tests.RemoveRange(_db.Tests.Where(temp => temp.Id == testId));
        return (await _db.SaveChangesAsync()) > 0;
    }

    public async Task<bool> RemoveJudgerFromTestAsync(Guid judgerId)
    {
        _db.Judgers.RemoveRange(_db.Judgers.Where(temp => temp.Id == judgerId));
        return (await _db.SaveChangesAsync()) > 0;
    }
}