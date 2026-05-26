namespace SensoryAnalysis.Entities;
public class Sample
{
    public Guid Id { get; set; }
    public Guid JudgerId { get; set; }
    public int Number { get; set; }
    public SampleTypes SampleType { get; set; }

    public Sample(Guid judgerId, int number, SampleTypes sampleType)
    {
        Id = Guid.NewGuid();
        JudgerId = judgerId;
        Number = number;
        SampleType = sampleType;
    }
}