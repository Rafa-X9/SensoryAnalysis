namespace SensoryAnalysis.Entities;
public class Test
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public TestTypes TestType { get; set; }
    public List<Judger> Judgers { get; set; }
    public DateTime CreatedAt { get; set; }

    public Test(string name, TestTypes testType)
    {
        Id = Guid.NewGuid();
        Name = name;
        TestType = testType;
        Judgers = [];
        CreatedAt = DateTime.Now;
    }
}