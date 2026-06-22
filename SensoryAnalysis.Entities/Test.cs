using System.ComponentModel.DataAnnotations;

namespace SensoryAnalysis.Entities;
public class Test
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(100)]
    public string Name { get; set; }
    public TestTypes TestType { get; set; }
    public Significances Significance { get; set; }
    public List<Judger> Judgers { get; set; }
    public DateTime CreatedAt { get; set; }

    [StringLength(50)]
    public string? NameOfSample1 { get; set; }

    [StringLength(50)]
    public string? NameOfSample2 { get; set; }

    public int? JudgerCount { get; set; }

    public Test()
    {
        Name = string.Empty;
        Judgers = [];
        JudgerCount = 0;
    }

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
        JudgerCount = Judgers.Count;
    }
}