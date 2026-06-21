using Microsoft.Extensions.Logging;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services.Helpers;
using System.Text;

namespace SensoryAnalysis.Services;

/// <summary>
/// The <see cref="ITestService"/> implementation for triangular tests
/// </summary>
public class TriangularTestService : ITestService
{
    private readonly Random _random;
    private readonly ILogger<TriangularTestService> _logger;

    public TriangularTestService(ILogger<TriangularTestService> logger)
    {
        _random = new();
        _logger = logger;
    }

    public bool IsValid(TestAddRequest request)
    {
        return request.TestType == TestTypes.Triangular;
    }

    public List<Sample> GenerateSamples(Guid? judgerId = null, SampleTypes? differentSample = null)
    {
        _logger.LogSampleGeneration(judgerId, differentSample);

        List<int> numbers = [];
        while (numbers.Count < 3)
        {
            int n = _random.Next(100, 999);
            if (!n.IsIn(100, 333, 666, 777, 999)
                && !numbers.Contains(n)
                && !numbers.Any(number => number % 100 == n % 100)
                && !numbers.Any(number => number / 100 == n / 100))
            {
                numbers.Add(n);
            }
        }

        while ((numbers[0] < numbers[1] && numbers[1] < numbers[2])
            || (numbers[0] > numbers[1] && numbers[1] > numbers[2]))
        {
            numbers.Sort((n1, n2) => _random.Next(-2, 3));
        }

        List<Sample> samples = [];
        Guid id = judgerId ?? Guid.NewGuid();
        int differentPosition = _random.Next(0, 3);
        differentSample ??= Enum.Parse<SampleTypes>(_random.Next(1, 3).ToString());
        SampleTypes doubleSample = (differentSample == SampleTypes.Sample1) ? SampleTypes.Sample2 : SampleTypes.Sample1;

        for (int i = 0; i <= 2; i++)
        {
            //SampleTypes type = (i == differentPosition) ? doubleSample : (SampleTypes)differentSample;
            SampleTypes type = (i == differentPosition) ? (SampleTypes)differentSample : doubleSample;
            samples.Add(new(id, numbers[i], type));
        }

        _logger.LogSampleResults(samples);

        return samples;
    }

    public TestResult GetTestResult(Test test)
    {
        _logger.LogTestResultGeneration(test);

        double n = test.Judgers.Count(judger => judger.Answer != null);
        if (n == 0)
        {
            return new(test.Judgers.Count, 0, 0, 1);
        }

        //the formula for calculating the minimum answers is shown in
        //SensoryAnalysis.Tests.TriangularTestTests in GetTestResults region

        double z = test.Significance switch
        {
            Significances._20 => 0.84,
            Significances._10 => 1.28,
            Significances._5 => 1.64,
            Significances._1 => 2.33,
            Significances._01 => 3.10,
            _ => throw new ArgumentException("Invalid significance level"),
        };

        int minimumAnswers = (int)Math.Ceiling((n / 3) + (z * Math.Sqrt(2 * n / 9)));

        int correctAnswers = TestHelpers.CorrectAnswerCount(test.Judgers);

        TestResult result = new(test.Judgers.Count,
            Convert.ToInt32(n),
            correctAnswers,
            minimumAnswers);

        _logger.LogTestResult(result);

        return result;
    }

    public TestResponse GetTestResponse(Test test)
    {
        return test.ToTestResponse();
    }

    public string Instructions()
    {
        return "Você está recebendo 3 amostras codificadas. " +
            "Duas amostras são iguais e uma diferente. Por favor, avalie " +
            "as amostras da esquerda para a direita e marque a amostra " +
            "diferente.";
    }

    public string SamplesInfo(Judger judger, Test test)
    {
        string numbers = "";
        foreach (Sample sample in judger.Samples)
        {
            numbers += TestHelpers.SampleTypeNumber(sample.SampleType);
        }
        return numbers;
    }
}