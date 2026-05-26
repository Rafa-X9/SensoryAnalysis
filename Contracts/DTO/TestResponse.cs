using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Contracts.DTO;
public class TestResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public TestTypes TestType { get; set; }
    public List<JudgerResponse> Judgers { get; set; }
    public DateTime CreatedAt { get; set; }

    public TestResponse(Guid id,
        string name,
        TestTypes testType,
        List<Judger> judgers,
        DateTime createdAt)
    {
        Id = id;
        Name = name;
        TestType = testType;
        Judgers = judgers.Select(judger => judger.ToJudgerResponse()).ToList();
        CreatedAt = createdAt;
    }
}