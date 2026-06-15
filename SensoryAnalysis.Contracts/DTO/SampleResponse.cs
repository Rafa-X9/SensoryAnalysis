using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Contracts.DTO;
public class SampleResponse
{
    public Guid Id { get; set; }
    public Guid JudgerId { get; set; }
    public int Number { get; set; }
    public SampleTypes SampleType { get; set; }

    public SampleResponse(Guid id, Guid judgerId, int number, SampleTypes sampleType)
    {
        Id = id;
        JudgerId = judgerId;
        Number = number;
        SampleType = sampleType;
    }
}