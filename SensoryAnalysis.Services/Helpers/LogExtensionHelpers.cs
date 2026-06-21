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
            $"has reached {typeof(T)}");

        logger.LogDebug("Sample generation info:\n" +
            $"          Judger id: {judgerId?.ToString() ?? "null"}\n" +
            $"          Different sample: {differentSample?.ToString() ?? "null"}");
    }

    internal static void LogSampleResults<T>(this ILogger<T> logger,
        IEnumerable<Sample> samples) where T : ITestService
    {
        logger.LogDebug("Generated samples:\n" +
            $"          {string.Join("\n          ", samples.Select(s => $"{s.Number}: {s.SampleType.ToString()}"))}");
    }

    internal static void LogTestResultGeneration<T>(this ILogger<T> logger,
        Test test) where T : ITestService
    {
        logger.LogInformation($"A request to get {test.Id} test's results has " +
            $"reached {typeof(T)}");
    }

    internal static void LogTestResult<T>(this ILogger<T> logger,
        TestResult result) where T : ITestService
    {
        logger.LogDebug("Test results info:\n" +
            $"          Total judgers: {result.TotalJudgers}\n" +
            $"          Total answers: {result.TotalAnswers}\n" +
            $"          Correct answers: {result.CorrectAnswers}\n" +
            $"          Wrong answers: {result.WrongAnswers}\n" +
            $"          Minimum for relevance: {result.MinimumForRelevance}\n" +
            $"          Has relevance: {result.HasRelevance}");
    }
}