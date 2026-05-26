namespace SensoryAnalysis.Contracts.DTO;
public class TestResult
{
    /// <summary>
    /// The total amount of judgers in the test
    /// </summary>
    public int TotalJudgers { get; set; }

    /// <summary>
    /// The total amount of valid answers in the test
    /// </summary>
    public int TotalAnswers { get; set; }

    /// <summary>
    /// The total amount of correct answers in the test
    /// </summary>
    public int CorrectAnswers { get; set; }

    /// <summary>
    /// The total amount of wrong answers in the test from the ones who answered
    /// </summary>
    public int WrongAnswers { get { return TotalAnswers - CorrectAnswers; } }
    
    /// <summary>
    /// The minimum amount of correct answers needed to assert relevance in the test
    /// </summary>
    public int MinimumForRelevance { get; set; }

    /// <summary>
    /// Whether the test has the minimum amount of correct answers needed to assert relevance
    /// </summary>
    public bool HasRelevance { get { return CorrectAnswers >= MinimumForRelevance; } }

    public TestResult(int totalJudgers, int totalAnswers, int correctAnswers, int minimumForRelevance)
    {
        TotalJudgers = totalJudgers;
        TotalAnswers = totalAnswers;
        CorrectAnswers = correctAnswers;
        MinimumForRelevance = minimumForRelevance;
    }
}