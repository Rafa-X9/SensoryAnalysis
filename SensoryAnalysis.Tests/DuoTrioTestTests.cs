using Microsoft.Extensions.Logging;
using Moq;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services;
using SensoryAnalysis.Services.Helpers;
using System.Threading.Tasks;

namespace SensoryAnalysis.Tests;
public class DuoTrioTestTests
{
    private readonly ITestService _testService;
    private readonly ITestManagerService _manager;
    private readonly ITestRepository _repository;

    public DuoTrioTestTests()
    {
        var testServiceLoggerMock = new Mock<ILogger<DuoTrioTestService>>();
        var testServiceLogger = testServiceLoggerMock.Object;

        var managerLoggerMock = new Mock<ILogger<TestManagerService>>();
        var managerLogger = managerLoggerMock.Object;

        var serviceFactoryMock = new Mock<ITestServiceFactory>();
        var serviceFactory = serviceFactoryMock.Object;

        _repository = new InMemoryRepository();
        _testService = new DuoTrioTestService(testServiceLogger);
        _manager = new TestManagerService(_repository, serviceFactory, managerLogger);

        serviceFactoryMock
            .Setup(temp => temp.GetTestService(It.IsAny<TestTypes>()))
            .Returns(_testService);
    }

    #region IsValid

    [Fact]
    public void IsValid_WrongType_ReturnsFalse()
    {
        TestAddRequest wrongType = new("Test with wrong type", TestTypes.Triangular, Significances._5);
        Assert.False(_testService.IsValid(wrongType));
    }

    [Fact]
    public void IsValid_RightType_ReturnsTrue()
    {
        TestAddRequest rightType = new("Test with right type", TestTypes.DuoTrio, Significances._1);
        Assert.True(_testService.IsValid(rightType));
    }

    #endregion

    #region GenerateSamples

    /*
     * Duo-trio tests show a reference sample and two numbered samples, with the
     * judger having to mark down the one equal to the reference
     * 
     * This project has the last sample as the reference, its number being 0
     * 
     */

    [Fact]
    public void GenerateSamples_GeneratesThreeSamples()
    {
        List<Sample> samples = _testService.GenerateSamples();
        Assert.Equal(3, samples.Count);
    }

    [Fact]
    public void GenerateSamples_ThirdSampleHasNumberZero()
    {
        List<Sample> samples = _testService.GenerateSamples();
        Assert.Equal(0, samples[2].Number);
    }

    [Fact]
    public void GenerateSamples_GeneratesThreeDigitNumbers()
    {
        List<Sample> samples = _testService.GenerateSamples();
        Assert.InRange(samples[0].Number, 101, 998);
        Assert.InRange(samples[1].Number, 101, 998);
    }

    [Fact]
    public void GenerateSamples_DoesNotGenerateForbiddenNumbers()
    {
        List<Sample> samples = _testService.GenerateSamples();
        Assert.DoesNotContain(samples, s => s.Number.IsIn(100, 333, 666, 777, 999));
    }

    [Fact]
    public void GenerateSamples_GeneratesExactlyOneSampleEqualToReference()
    {
        List<Sample> samples = _testService.GenerateSamples();
        Sample reference = samples[2];

        Assert.NotEqual(samples[0].SampleType, samples[1].SampleType);
        Assert.True((samples[0].SampleType == reference.SampleType)
            || (samples[1].SampleType == reference.SampleType));
    }

    [Fact]
    public void GenerateSamples_GivenSampleType_UsesGivenSampleType()
    {
        List<Sample> samples = _testService.GenerateSamples(differentSample: SampleTypes.Sample1);
        Assert.Single(samples, sample => sample.SampleType == SampleTypes.Sample1);
    }

    #endregion

    #region GetTestResults

    [Fact]
    public async Task GetTestResults_NotEnoughCorrectAnswers_ReturnsFailure()
    {
        TestAddRequest addRequest = new("Test 1", TestTypes.DuoTrio, Significances._5);
        TestResponse? response = await _manager.AddTestAsync(addRequest);

        await _manager.AddJudgersToTestAsync(response.Id, 21);
        response = await _manager.GetTestByIdAsync(response.Id);
        if (response is null) throw new Exception();
        List<JudgerResponse> judgers = response.Judgers;

        //this is used to access the samples information we can't access in the response
        Test? test = await _repository.GetTestByIdAsync(response.Id);
        if (test is null) throw new Exception();

        //14 judgers answering correctly
        for (int i = 0; i < 14; i++)
        {
            string info = _testService.SamplesInfo(test.Judgers[i], test);
            int differentSample = judgers[i].Samples.First(s => s.SampleType != test.Judgers[i].Samples[2].SampleType).Number;
            await _manager.AddAnswerToTestAsync(response.Id, judgers[i].Id, differentSample);
        }

        //6 remaining answering incorrectly, the last 1 didn't answer
        for (int i = 14; i < 20; i++)
        {
            int equalSample = judgers[i].Samples.First(s => s.SampleType == test.Judgers[i].Samples[2].SampleType).Number;
            await _manager.AddAnswerToTestAsync(response.Id, judgers[i].Id, equalSample);
        }

        TestResult? result = (await _manager.GetTestResultsAsync(response.Id))?.Result;
        Assert.NotNull(result);
        Assert.Equal(21, result.TotalJudgers);
        Assert.Equal(20, result.TotalAnswers);
        Assert.Equal(14, result.CorrectAnswers);
        Assert.Equal(6, result.WrongAnswers);
        Assert.Equal(15, result.MinimumForRelevance);
        Assert.False(result.HasRelevance);
    }

    [Fact]
    public async Task GetTestResults_EnoughCorrectAnswers_ReturnsRelevant()
    {
        TestAddRequest addRequest = new("Test 2", TestTypes.DuoTrio, Significances._5);
        TestResponse? response = await _manager.AddTestAsync(addRequest);
        await _manager.AddJudgersToTestAsync(response.Id, 21);

        response = await _manager.GetTestByIdAsync(response.Id);
        if (response is null) throw new Exception();
        List<JudgerResponse> judgers = response.Judgers;

        Test? test = await _repository.GetTestByIdAsync(response.Id);
        if (test is null) throw new Exception();

        //15 judgers answering correctly
        for (int i = 0; i < 15; i++)
        {
            string info = _testService.SamplesInfo(test.Judgers[i], test);
            int differentSample = judgers[i].Samples.First(s => s.SampleType != test.Judgers[i].Samples[2].SampleType).Number;
            await _manager.AddAnswerToTestAsync(response.Id, judgers[i].Id, differentSample);
        }

        //5 remaining answering incorrectly, the last 1 didn't answer
        for (int i = 15; i < 20; i++)
        {
            int equalSample = judgers[i].Samples.First(s => s.SampleType == test.Judgers[i].Samples[2].SampleType).Number;
            await _manager.AddAnswerToTestAsync(response.Id, judgers[i].Id, equalSample);
        }

        TestResult? result = (await _manager.GetTestResultsAsync(response.Id))?.Result;
        Assert.NotNull(result);
        Assert.Equal(21, result.TotalJudgers);
        Assert.Equal(20, result.TotalAnswers);
        Assert.Equal(15, result.CorrectAnswers);
        Assert.Equal(5, result.WrongAnswers);
        Assert.Equal(15, result.MinimumForRelevance);
        Assert.True(result.HasRelevance);
    }

    #endregion
}