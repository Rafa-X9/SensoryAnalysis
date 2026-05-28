namespace SensoryAnalysis.Entities;
public class Test
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public TestTypes TestType { get; set; }
    public Significances Significance { get; set; }
    public List<Judger> Judgers { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? NameOfSample1 { get; set; }
    public string? NameOfSample2 { get; set; }

    public Test(string name,
        TestTypes testType,
        Significances sensitivity,
        string? nameOfSample1 = null,
        string? nameOfSample2 = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        TestType = testType;
        Significance = sensitivity;
        Judgers = [];
        CreatedAt = DateTime.Now;
        NameOfSample1 = nameOfSample1;
        NameOfSample2 = nameOfSample2;
    }
}