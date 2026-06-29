using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services;
using SensoryAnalysis.Services.Helpers;
using Moq;

namespace SensoryAnalysis.Tests;

/// <summary>
/// Test cases for SensoryAnalysis.Services.TriangularTestService
/// </summary>
public class TriangularTestTests
{
    private readonly ITestManagerService _manager;
    private readonly ITestService _testService;

    public TriangularTestTests()
    {
        var testServiceLoggerMock = new Mock<ILogger<TriangularTestService>>();
        var testServiceLogger = testServiceLoggerMock.Object;

        var managerLoggerMock = new Mock<ILogger<TestManagerService>>();
        var managerLogger = managerLoggerMock.Object;

        var serviceFactoryMock = new Mock<ITestServiceFactory>();
        var serviceFactory = serviceFactoryMock.Object;

        _manager = new TestManagerService(new InMemoryRepository(), serviceFactory, managerLogger);
        _testService = new TriangularTestService(testServiceLogger);

        serviceFactoryMock
            .Setup(temp => temp.GetTestService(It.IsAny<TestTypes>()))
            .Returns(_testService);
    }

    #region IsValid

    //The test type must be triangular

    [Fact]
    public void IsValid_Invalid_ReturnsFalse()
    {
        TestAddRequest wrongType = new("Test with wrong type", TestTypes.DuoTrio, Significances._5);
        Assert.False(_testService.IsValid(wrongType));
    }

    [Fact]
    public void IsValid_Invalid_ReturnsTrue()
    {
        TestAddRequest rightType = new("Test with right type", TestTypes.Triangular, Significances._1);
        Assert.True(_testService.IsValid(rightType));
    }

    #endregion

    #region GenerateSamples

    /*
     * Triangular tests are made with each judger having 3 samples, two 
     * of them being of the same type and the remaining one being not
     * 
     * Each sample is assigned a 3-digit number to identify. The numbers
     * must not repeat, not be in ascending nor descending order.
     * 
     * If a SampleType is given, the different sample must be of it
     * 
     * Certain numbers must be avoided to prevent bias on the judger, they
     * are 100, 333, 666, 777, and 999 as far as I'm concerned
     * 
     */

    [Fact]
    public void GenerateSamples_GeneratesThreeSamples()
    {
        List<Sample> samples = _testService.GenerateSamples();
        Assert.Equal(3, samples.Count);
    }

    [Fact]
    public void GenerateSamples_GeneratesOnlyThreeDigitNumbers()
    {
        List<Sample> samples = _testService.GenerateSamples();
        foreach (Sample sample in samples)
        {
            Assert.InRange(sample.Number, 100, 999);
        }
    }

    [Fact]
    public void GenerateSamples_GeneratesNoRepetitions()
    {
        List<Sample> samples = _testService.GenerateSamples();
        foreach (Sample sample in samples)
        {
            Assert.DoesNotContain(samples, s => s.Number == sample.Number && s.Id != sample.Id);
        }
    }

    [Fact]
    public void GenerateSamples_DoesNotGenerateAscendingNorDescendingNumbers()
    {
        List<Sample> samples = _testService.GenerateSamples();
        bool isAscending = true;
        for (int i = 1; i < samples.Count; i++)
        {
            if (samples[i - 1].Number > samples[i].Number)
            {
                isAscending = false;
            }
        }
        Assert.False(isAscending);

        bool isDescending = true;
        for (int i = 1; i < samples.Count; i++)
        {
            if (samples[i - 1].Number < samples[i].Number)
            {
                isDescending = false;
            }
        }
        Assert.False(isDescending);
    }

    [Fact]
    public void GenerateSamples_GeneratesTwoEqualSamples()
    {
        List<Sample> samples = _testService.GenerateSamples();
        int s1 = samples.Count(s => s.SampleType == SampleTypes.Sample1);
        int s2 = samples.Count(s => s.SampleType == SampleTypes.Sample2);
        Assert.True((s1 == 1 && s2 == 2) || (s1 == 2 && s2 == 1));
    }

    [Fact]
    public void GenerateSamples_DoesNotGenerateForbiddenNumbers()
    {
        List<Sample> samples = _testService.GenerateSamples();
        Assert.DoesNotContain(samples, s => s.Number.IsIn(100, 333, 666, 777, 999));
    }

    [Fact]
    public void GenerateSamples_GivenSampleType_UsesGivenSampleType()
    {
        List<Sample> samples = _testService.GenerateSamples(differentSample: SampleTypes.Sample1);
        Assert.Single(samples, sample => sample.SampleType == SampleTypes.Sample1);
    }

    #endregion

    #region GetTestResults

    /*
     * I am using a sheet with a table my teacher gave.
     * It has the following formula for the minimum number of correct
     * responses to assert relevance, the result is rounded up:
     * 
     * x = (n / 3) + (z * sqrt(2n / 9))
     * 
     * Where:
     * x is the minimum amount of responses
     * n is the total amount of answers
     * z is a factor that varies with the significance level:
     * _ 0.84 for 20%,
     * _ 1.28 for 10%,
     * _ 1.64 for 5%,
     * _ 2.33 for 1%, and
     * _ 3.10 for .1%
     * 
     */

    [Fact]
    public async Task GetTestResults_EnoughCorrectAnswers_ReturnsRelevant()
    {
        TestAddRequest addRequest = new("Test 1", TestTypes.Triangular, Significances._5);
        TestResponse? response = await _manager.AddTestAsync(addRequest);
        await _manager.AddJudgersToTestAsync(response.Id, 21);
        response = await _manager.GetTestByIdAsync(response.Id);
        if (response is null) throw new Exception();
        List<JudgerResponse> judgers = response.Judgers;

        //11 judgers answering correctly
        for (int i = 0; i < 11; i++)
        {
            int differentSample;
            if (judgers[i].Samples.Count(s => s.SampleType == SampleTypes.Sample1) == 1)
            {
                differentSample = judgers[i].Samples.First(s => s.SampleType == SampleTypes.Sample1).Number;
            }
            else
            {
                differentSample = judgers[i].Samples.First(s => s.SampleType == SampleTypes.Sample2).Number;
            }
            await _manager.AddAnswerToTestAsync(response.Id, judgers[i].Id, differentSample);
        }

        //9 remaining answering incorrectly, the last 1 didn't answer
        for (int i = 11; i < 20; i++)
        {
            int differentSample;
            if (judgers[i].Samples.Count(s => s.SampleType == SampleTypes.Sample1) == 1)
            {
                differentSample = judgers[i].Samples.First(s => s.SampleType == SampleTypes.Sample2).Number;
            }
            else
            {
                differentSample = judgers[i].Samples.First(s => s.SampleType == SampleTypes.Sample1).Number;
            }
            await _manager.AddAnswerToTestAsync(response.Id, judgers[i].Id, differentSample);
        }

        TestResult? result = (await _manager.GetTestResultsAsync(response.Id))?.Result;
        Assert.NotNull(result);
        Assert.Equal(21, result.TotalJudgers);
        Assert.Equal(20, result.TotalAnswers);
        Assert.Equal(11, result.CorrectAnswers);
        Assert.Equal(9, result.WrongAnswers);
        Assert.Equal(11, result.MinimumForRelevance);
        Assert.True(result.HasRelevance);
    }

    #endregion
}