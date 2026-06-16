using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services;

namespace SensoryAnalysis.Tests;
public class DuoTrioTestTests
{
    private readonly ITestService _testService = new DuoTrioTestService();
    private readonly ITestManagerService _manager = new TestManagerService(new TestServiceFactory(), null, false);

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
    public void GetTestResults_1()
    {
        TestAddRequest addRequest = new("Test 1", TestTypes.DuoTrio, Significances._5);
        TestResponse response = _manager.AddTest(addRequest);
        for (int i = 0; i < 21; i++)
        {
            _manager.AddJudgerToTest(response.Id);
        }
        List<JudgerResponse> judgers = _manager.GetJudgersFromTest(response.Id);

        //14 judgers answering correctly
        for (int i = 0; i < 14; i++)
        {
            int differentSample = judgers[i].Samples.First(s => s.SampleType != judgers[i].Samples[2].SampleType).Number;
            _manager.AddAnswerToTest(response.Id, judgers[i].Id, differentSample);
        }

        //6 remaining answering incorrectly, the last 1 didn't answer
        for (int i = 14; i < 20; i++)
        {
            int equalSample = judgers[i].Samples.First(s => s.SampleType == judgers[i].Samples[2].SampleType).Number;
            _manager.AddAnswerToTest(response.Id, judgers[i].Id, equalSample);
        }

        TestResult result = _manager.GetTestResults(response.Id);
        Assert.Equal(21, result.TotalJudgers);
        Assert.Equal(20, result.TotalAnswers);
        Assert.Equal(14, result.CorrectAnswers);
        Assert.Equal(6, result.WrongAnswers);
        Assert.Equal(15, result.MinimumForRelevance);
        Assert.False(result.HasRelevance);
    }

    [Fact]
    public void GetTestResults_2()
    {
        TestAddRequest addRequest = new("Test 2", TestTypes.DuoTrio, Significances._5);
        TestResponse response = _manager.AddTest(addRequest);
        for (int i = 0; i < 21; i++)
        {
            _manager.AddJudgerToTest(response.Id);
        }
        List<JudgerResponse> judgers = _manager.GetJudgersFromTest(response.Id);

        //15 judgers answering correctly
        for (int i = 0; i < 15; i++)
        {
            int differentSample = judgers[i].Samples.First(s => s.SampleType != judgers[i].Samples[2].SampleType).Number;
            _manager.AddAnswerToTest(response.Id, judgers[i].Id, differentSample);
        }

        //5 remaining answering incorrectly, the last 1 didn't answer
        for (int i = 15; i < 20; i++)
        {

            int differentSample = judgers[i].Samples.First(s => s.SampleType == judgers[i].Samples[2].SampleType).Number;
            _manager.AddAnswerToTest(response.Id, judgers[i].Id, differentSample);
        }

        TestResult result = _manager.GetTestResults(response.Id);
        Assert.Equal(21, result.TotalJudgers);
        Assert.Equal(20, result.TotalAnswers);
        Assert.Equal(15, result.CorrectAnswers);
        Assert.Equal(5, result.WrongAnswers);
        Assert.Equal(15, result.MinimumForRelevance);
        Assert.True(result.HasRelevance);
    }

    #endregion
}