using System.ComponentModel.DataAnnotations;

namespace SensoryAnalysis.Entities;
public class Sample
{
    [Key]
    public Guid Id { get; set; }
    public Guid JudgerId { get; set; }
    public Judger? SamplesJudger { get; set; }
    public int Number { get; set; }
    public SampleTypes SampleType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Sample() { }

    public Sample(Guid judgerId, int number, SampleTypes sampleType)
    {
        Id = Guid.NewGuid();
        JudgerId = judgerId;
        Number = number;
        SampleType = sampleType;
    }
}