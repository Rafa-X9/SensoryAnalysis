using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services.Helpers;

namespace SensoryAnalysis.Services;

/// <summary>
/// The <see cref="ITestService"/> implementation for triangular tests
/// </summary>
public class TriangularTestService : ITestService
{
    private readonly Random _random;

    public TriangularTestService()
    {
        _random = new();
    }

    public bool IsValid(TestAddRequest request)
    {
        return request.TestType == TestTypes.Triangular;
    }

    public List<Sample> GenerateSamples(Guid? judgerId = null, SampleTypes? differentSample = null)
    {
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
        return samples;
    }

    public TestResult GetTestResult(Test test)
    {
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

        int correctAnswers = 0;
        foreach (Judger judger in test.Judgers)
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

        return new(test.Judgers.Count,
            Convert.ToInt32(n),
            correctAnswers,
            minimumAnswers);
    }

    public TestResponse GetTestResponse(Test test)
    {
        return test.ToTestResponse();
    }
}