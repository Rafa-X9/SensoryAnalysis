using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Contracts.DTO;
public class JudgerResponse
{
    public Guid Id { get; set; }
    public List<Sample> Samples { get; set; }
    public int? Answer { get; set; }

    public JudgerResponse(Guid id, List<Sample> samples, int? answer)
    {
        Id = id;
        Samples = samples;
        Answer = answer;
    }
}

public static class JudgerExtension
{
    public static JudgerResponse ToJudgerResponse(this Judger judger)
    {
        return new(judger.Id, judger.Samples, judger.Answer);
    }
}