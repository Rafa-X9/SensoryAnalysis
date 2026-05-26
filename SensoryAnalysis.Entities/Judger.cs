namespace SensoryAnalysis.Entities;
public class Judger
{
    public Guid Id { get; set; }
    public Guid TestId { get; set; }
    public List<Sample> Samples { get; set; }
    public int? Answer { get; set; }

    public Judger(Guid testId, List<Sample> samples)
    {
        Id = Guid.NewGuid();
        TestId = testId;
        Samples = samples;
        Answer = null;
    }
}