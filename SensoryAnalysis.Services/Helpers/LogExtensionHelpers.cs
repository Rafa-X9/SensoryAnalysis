using Microsoft.Extensions.Logging;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Services.Helpers;
internal static class LogExtensionHelpers
{
    internal static void LogSampleGeneration<T>(this ILogger<T> logger,
        Guid? judgerId, SampleTypes? differentSample) where T : ITestService
    {
        logger.LogInformation("A request to generate the samples for a judger " +
            "has reached {TestServiceType}" +
            "          Judger id: {JudgerId}\n" +
            "          Different sample: {DifferentSample}",
            typeof(T),
            judgerId?.ToString() ?? "null",
            differentSample?.ToString() ?? "null");
    }

    internal static void LogSampleResults<T>(this ILogger<T> logger,
        IEnumerable<Sample> samples) where T : ITestService
    {
        logger.LogInformation("Generated samples:\n" +
            "          {GeneratedSamples}",
            string.Join("\n          ", samples.Select(s => $"{s.Number}: {s.SampleType.ToString()}")));
    }

    internal static void LogTestResultGeneration<T>(this ILogger<T> logger,
        Test test) where T : ITestService
    {
        logger.LogInformation("A request to get {TestId} test's results has " +
            "reached {TestServiceType}",
            test.Id, typeof(T));
    }

    internal static void LogTestResult<T>(this ILogger<T> logger,
        TestResult result) where T : ITestService
    {
        logger.LogInformation("Test results info:\n" +
            "          Total judgers: {ResultTotalJudgers}\n" +
            "          Total answers: {ResultTotalAnswers}\n" +
            "          Correct answers: {ResultCorrectAnswers}\n" +
            "          Wrong answers: {ResultWrongAnswers}\n" +
            "          Minimum for relevance: {ResultMinimumForRelevance}\n" +
            "          Has relevance: {ResultHasRelevance}",
            
            result.TotalJudgers,
            result.TotalAnswers,
            result.CorrectAnswers,
            result.WrongAnswers,
            result.MinimumForRelevance,
            result.HasRelevance);
    }
}