using Microsoft.Extensions.Logging;
using Moq;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services;
using System.Threading.Tasks;

namespace SensoryAnalysis.Tests;
public class DuoTrioTestTests
{
    private readonly ITestService _testService;
    private readonly ITestManagerService _manager;

    public DuoTrioTestTests()
    {
        var testServiceLoggerMock = new Mock<ILogger<DuoTrioTestService>>();
        var testServiceLogger = testServiceLoggerMock.Object;

        var managerLoggerMock = new Mock<ILogger<TestManagerService>>();
        var managerLogger = managerLoggerMock.Object;

        var serviceFactoryMock = new Mock<ITestServiceFactory>();
        var serviceFactory = serviceFactoryMock.Object;

        _testService = new DuoTrioTestService(testServiceLogger);
        _manager = new TestManagerService(new InMemoryRepository(), serviceFactory, managerLogger);

        serviceFactoryMock
            .Setup(temp => temp.GetTestService(It.IsAny<TestTypes>()))
            .Returns(_testService);
    }

    #region IsValid

    //The test type must be DuoTrio

    [Fact]
    public void IsValid()
    {
        TestAddRequest wrongType = new("Test with wrong type", TestTypes.Triangular, Significances._5);
        TestAddRequest rightType = new("Test with right type", TestTypes.DuoTrio, Significances._1);

        Assert.False(_testService.IsValid(wrongType));
        Assert.True(_testService.IsValid(rightType));
    }

    #endregion

    #region GetTestResults

    [Fact]
    public async Task GetTestResults_1()
    {
        TestAddRequest addRequest = new("Test 1", TestTypes.DuoTrio, Significances._5);
        TestResponse response = await _manager.AddTestAsync(addRequest);
        for (int i = 0; i < 21; i++)
        {
            await _manager.AddJudgerToTestAsync(response.Id);
        }
        List<JudgerResponse> judgers = await _manager.GetJudgersFromTestAsync(response.Id);

        //14 judgers answering correctly
        for (int i = 0; i < 14; i++)
        {
            int differentSample = judgers[i].Samples.First(s => s.SampleType != judgers[i].Samples[2].SampleType).Number;
            await _manager.AddAnswerToTestAsync(response.Id, judgers[i].Id, differentSample);
        }

        //6 remaining answering incorrectly, the last 1 didn't answer
        for (int i = 14; i < 20; i++)
        {
            int equalSample = judgers[i].Samples.First(s => s.SampleType == judgers[i].Samples[2].SampleType).Number;
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
    public async Task GetTestResults_2()
    {
        TestAddRequest addRequest = new("Test 2", TestTypes.DuoTrio, Significances._5);
        TestResponse response = await _manager.AddTestAsync(addRequest);
        for (int i = 0; i < 21; i++)
        {
            await _manager.AddJudgerToTestAsync(response.Id);
        }
        List<JudgerResponse> judgers = await _manager.GetJudgersFromTestAsync(response.Id);

        //15 judgers answering correctly
        for (int i = 0; i < 15; i++)
        {
            int differentSample = judgers[i].Samples.First(s => s.SampleType != judgers[i].Samples[2].SampleType).Number;
            await _manager.AddAnswerToTestAsync(response.Id, judgers[i].Id, differentSample);
        }

        //5 remaining answering incorrectly, the last 1 didn't answer
        for (int i = 15; i < 20; i++)
        {

            int differentSample = judgers[i].Samples.First(s => s.SampleType == judgers[i].Samples[2].SampleType).Number;
            await _manager.AddAnswerToTestAsync(response.Id, judgers[i].Id, differentSample);
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