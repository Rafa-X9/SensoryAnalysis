using SensoryAnalysis.Entities;
using static System.Net.Mime.MediaTypeNames;

namespace SensoryAnalysis.Services.Helpers;
internal static class TestResultHelpers
{
    internal static int CorrectAnswerCount(List<Judger> judgers)
    {
        int correctAnswers = 0;
        foreach (Judger judger in judgers)
        {
            if (judger.Answer is null) continue;
            Sample? sample = judger.Samples.FirstOrDefault(s => s.Number == judger.Answer);
            if (sample is null)
            {
                throw new ArgumentException("A judger did not choose a correct number");
            }
            if (judger.Samples.Count(s => s.SampleType == sample.SampleType) == 1)
            {
                correctAnswers++;
            }
        }
        return correctAnswers;
    }

    internal static double SignificanceToDouble(Significances significance)
    {
        return significance switch
        {
            Significances._20 => 0.20,
            Significances._10 => 0.10,
            Significances._5 => 0.05,
            Significances._1 => 0.01,
            Significances._01 => 0.001,
            _ => throw new ArgumentException("Invalid significance level"),
        };
    }
}