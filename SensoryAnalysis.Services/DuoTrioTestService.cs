using Microsoft.Extensions.Logging;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services.Helpers;

namespace SensoryAnalysis.Services;

/// <summary>
/// The <see cref="ITestService"/> implementation for duo-trio tests.
/// The third sample is the reference, it has the code of 0
/// </summary>
public class DuoTrioTestService : ITestService
{
    private readonly Random _random;
    private readonly ILogger<DuoTrioTestService> _logger;

    public DuoTrioTestService(ILogger<DuoTrioTestService> logger)
    {
        _random = new();
        _logger = logger;
    }

    public bool IsValid(TestAddRequest request)
    {
        return request.TestType == TestTypes.DuoTrio;
    }

    public List<Sample> GenerateSamples(Guid? judgerId = null, SampleTypes? differentSample = null)
    {
        _logger.LogSampleGeneration(judgerId, differentSample);

        judgerId ??= Guid.NewGuid();
        if (differentSample is null)
        {
            int number = _random.Next(0, 2);
            differentSample = number == 0 ? SampleTypes.Sample1 : SampleTypes.Sample2;
        }
        List<int> numbers = [];
        while (numbers.Count != 2)
        {
            int number = _random.Next(100, 999);
            if (!number.IsIn(100, 333, 666, 777, 999)
                && !numbers.Contains(number))
            {
                numbers.Add(number);
            }
        }
        int different = _random.Next(0, 2);
        List<Sample> samples = [];
        for (int i = 0; i < 2; i++)
        {
            SampleTypes type;
            if (i == different)
            {
                type = differentSample.Value;
            }
            else
            {
                type = differentSample.Value.OtherSampleType();
            }
            samples.Add(new(judgerId.Value, numbers[i], type));
        }
        samples.Add(new(judgerId.Value, 0, differentSample.Value.OtherSampleType()));

        _logger.LogSampleResults(samples);

        return samples;
    }

    public TestResult GetTestResult(Test test)
    {
        _logger.LogTestResultGeneration(test);

        int answerCount = test.Judgers.Count(j => j.Answer is not null);
        if (answerCount == 0)
        {
            return new(test.Judgers.Count, 0, 0, 1);
        }
        
        int correctAnswers = TestHelpers.CorrectAnswerCount(test.Judgers);
        double relevance = TestHelpers.SignificanceToDouble(test.Significance);

        TestResult result = new(test.Judgers.Count,
            answerCount,
            correctAnswers,
            MinimumForRelevance(answerCount, relevance));

        _logger.LogTestResult(result);

        return result;
    }

    public TestResponse GetTestResponse(Test test)
    {
        List<JudgerResponse> judgers = [];
        foreach (Judger judger in test.Judgers)
        {
            judgers.Add(new(judger.Id,
                [judger.Samples[0], judger.Samples[1]],
                judger.Answer));
        }
        TestResponse response = test.ToTestResponse();
        response.Judgers = judgers;
        return response;
    }

    public string Instructions()
    {
        return "Você receberá uma amostra de referência e duas amostras " +
            "codificadas. Uma é igual à referência e a outra é diferente. " +
            "Analise as amostras da esquerda para a direita e assinale a " +
            "que for diferente da referência.";
    }
    public string SamplesInfo(Judger judger, Test test)
    {
        if (test.TestType != TestTypes.DuoTrio) throw new ArgumentException(null, nameof(test));
        Judger? j = test.Judgers.FirstOrDefault(temp => temp.Id == judger.Id);
        if (j is null) throw new ArgumentException(null, nameof(judger));

        string numbers = "";
        for (int i = 0; i < 2; i++)
        {
            numbers += TestHelpers.SampleTypeNumber(j.Samples[i].SampleType);
        }
        numbers += $" - R{TestHelpers.SampleTypeNumber(j.Samples[2].SampleType)}";
        return numbers;
    }

    #region Helpers

    private int MinimumForRelevance(int totalJudgers, double significance)
    {
        const double p = 0.5;

        for (int correctAnswers = 0; correctAnswers <= totalJudgers; correctAnswers++)
        {
            double probability = 0;

            for (int k = correctAnswers; k <= totalJudgers; k++)
            {
                probability += BinomialProbability(totalJudgers, k, p);
            }

            if (probability <= significance)
            {
                return correctAnswers;
            }
        }

        return totalJudgers + 1;
    }

    private double BinomialProbability(int n, int k, double p)
    {
        return BinomialCoefficient(n, k) *
               Math.Pow(p, k) *
               Math.Pow(1 - p, n - k);
    }

    private double BinomialCoefficient(int n, int k)
    {
        if (k < 0 || k > n)
            return 0;

        if (k > n - k)
            k = n - k;

        double result = 1;

        for (int i = 1; i <= k; i++)
        {
            result *= (n - k + i);
            result /= i;
        }

        return result;
    }

    #endregion
}