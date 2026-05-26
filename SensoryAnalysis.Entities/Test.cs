namespace SensoryAnalysis.Entities;
public class Test
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public TestTypes TestType { get; set; }
    public Significances Significance { get; set; }
    public List<Judger> Judgers { get; set; }
    public DateTime CreatedAt { get; set; }

    public Test(string name, TestTypes testType, Significances sensitivity)
    {
        Id = Guid.NewGuid();
        Name = name;
        TestType = testType;
        Significance = sensitivity;
        Judgers = [];
        CreatedAt = DateTime.Now;
    }
}