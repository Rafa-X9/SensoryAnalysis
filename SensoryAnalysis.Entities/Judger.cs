using System.ComponentModel.DataAnnotations;

namespace SensoryAnalysis.Entities;
public class Judger
{
    [Key]
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public Test? JudgersTest { get; set; }
    public List<Sample> Samples { get; set; }
    public int? Answer { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Judger() { }

    public Judger(Guid testId, List<Sample> samples)
    {
        Id = Guid.NewGuid();
        TestId = testId;
        Samples = samples;
        Answer = null;
    }
}