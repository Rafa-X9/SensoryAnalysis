using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Contracts.DTO;
public class TestResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public TestTypes TestType { get; set; }
    public Significances Significance { get; set; }
    public List<JudgerResponse> Judgers { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? NameOfSample1 { get; set; }
    public string? NameOfSample2 { get; set; }

    public TestResponse(Guid id,
        string name,
        TestTypes testType,
        Significances significance,
        List<Judger> judgers,
        DateTime createdAt,
        string? nameOfSample1 = null,
        string? nameOfSample2 = null)
    {
        Id = id;
        Name = name;
        TestType = testType;
        Significance = significance;
        Judgers = judgers.Select(judger => judger.ToJudgerResponse()).ToList();
        CreatedAt = createdAt;
        NameOfSample1 = nameOfSample1;
        NameOfSample2 = nameOfSample2;
    }
}

public static class TestExtension
{
    public static TestResponse ToTestResponse(this Test test)
    {
        return new(test.Id,
            test.Name,
            test.TestType,
            test.Significance,
            test.Judgers,
            test.CreatedAt,
            nameOfSample1: test.NameOfSample1,
            nameOfSample2: test.NameOfSample2);
    }
}