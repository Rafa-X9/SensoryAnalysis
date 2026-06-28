using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SensoryAnalysis.Tests;
public class TestManagerTests
{
    private readonly ITestManagerService _manager;
    private readonly ITestRepository _repository;

    public TestManagerTests()
    {
        var serviceFactoryMock = new Mock<ITestServiceFactory>();
        var serviceFactory = serviceFactoryMock.Object;

        var loggerMock = new Mock<ILogger<TestManagerService>>();
        var logger = loggerMock.Object;

        _repository = new InMemoryRepository();
        _manager = new TestManagerService(_repository,
            serviceFactory,
            logger);

        serviceFactoryMock
            .Setup(temp => temp.GetTestService(It.IsAny<TestTypes>()))
            .Returns(new TriangularTestService(new Mock<ILogger<TriangularTestService>>().Object));
    }

    #region AddTestAsync

    [Fact]
    public async Task AddTestAsync_NullArgument_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _manager.AddTestAsync(null);
        });
    }

    [Fact]
    public async Task AddTestAsync_InvalidTestType_ThrowsArgumentException()
    {
        //as ITestServiceFactory is mocked to return a triangular test service,
        //any test type not triangular will be invalid

        TestAddRequest request = new("test", TestTypes.DuoTrio, Significances._1);
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _manager.AddTestAsync(request);
        });
    }

    [Fact]
    public async Task AddTestAsync_ValidObject_AddsTest()
    {
        TestAddRequest request = new("test", TestTypes.Triangular, Significances._1);
        TestResponse response = await _manager.AddTestAsync(request);
        Assert.Contains(response, await _manager.GetAllTestsAsync());
    }

    #endregion

    #region AddJudgersToTest and AddJudgersToTestAsync

    [Fact]
    public async Task AddJudgersToTestAsync_NegativeAmount_ThrowsArgumentOutOfRangeException()
    {
        TestResponse response = await SetUpTest();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await _manager.AddJudgersToTestAsync(response.Id, -1);
        });
    }

    [Fact]
    public async Task AddJudgersToTestAsync_ValidAmount_ShouldCreateJudgers()
    {
        TestResponse before = await SetUpTest();
        TestResponse after = await _manager.AddJudgersToTestAsync(before.Id, 5);
        Assert.Equal(5, after.Judgers.Count);

        HashSet<JudgerResponse> judgersSet = new(new JudgerResponseComparer());
        foreach (JudgerResponse judger in after.Judgers)
        {
            Assert.NotEmpty(judger.Samples);
            judgersSet.Add(judger);
        }
        Assert.Equal(5, judgersSet.Count);
    }

    #endregion

    #region GetTestByIdAsync

    [Fact]
    public async Task GetTestByIdAsync_NoMatch_ReturnsNull()
    {
        Assert.Null(await _manager.GetTestByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetTestByIdAsync_Match_ReturnsMatch()
    {
        TestResponse response = await SetUpTest();
        TestResponse? match = await _manager.GetTestByIdAsync(response.Id);
        Assert.NotNull(match);
        Assert.Equal(response.Id, match.Id);
        Assert.Equal(response.Name, match.Name);
        Assert.Equal(response.TestType, match.TestType);
    }

    #endregion

    #region GetAllTestsAsync

    [Fact]
    public async Task GetAllTestsAsync_Empty_ReturnsEmptyList()
    {
        Assert.Empty(await _manager.GetAllTestsAsync());
    }

    [Fact]
    public async Task GetAllTestsAsync_NotEmpty_ReturnTests()
    {
        List<TestResponse> responses = [];
        for (int i = 0; i < 3; i++)
        {
            responses.Add(await SetUpTest());
        }

        List<TestResponse> allTests = await _manager.GetAllTestsAsync();
        foreach (TestResponse response in responses)
        {
            Assert.Contains(response, allTests);
        }
    }

    #endregion

    #region DeleteTestAsync

    [Fact]
    public async Task DeleteTestAsync_NoMatch_ReturnsFalse()
    {
        Assert.False(await _manager.DeleteTestAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteTestAsync_Match_DeletesAndReturnsTrue()
    {
        TestResponse response = await SetUpTest();
        Assert.True(await _manager.DeleteTestAsync(response.Id));
        Assert.DoesNotContain(response, await _manager.GetAllTestsAsync());
    }

    #endregion

    #region RemoveJudgerFromTestAsync

    [Fact]
    public async Task RemoveJudgerFromTestAsync_NoMatch_ReturnsFalse()
    {
        TestResponse response = await SetUpTest();
        await _manager.AddJudgersToTestAsync(response.Id, 1);
        Assert.False(await _manager.RemoveJudgerFromTestAsync(response.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task RemoveJudgerFromTestAsync_Match_DeletesAndReturnsTrue()
    {
        TestResponse setUp = await SetUpTest();
        await _manager.AddJudgersToTestAsync(setUp.Id, 1);
        bool success = await _manager.RemoveJudgerFromTestAsync(setUp.Id, setUp.Judgers[0].Id);
        TestResponse? after = await _manager.GetTestByIdAsync(setUp.Id);

        Assert.True(success);
        Assert.NotNull(after);
        Assert.Empty(after.Judgers);
    }

    #endregion

    #region AddAnswerToTestAsync

    [Fact]
    public async Task AddAnswerToTestAsync_InvalidTestId_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _manager.AddAnswerToTestAsync(Guid.NewGuid(), Guid.NewGuid(), 123);
        });
    }

    [Fact]
    public async Task AddAnswerToTestAsync_NumberSampleGiven_AddsAnswer()
    {
        TestResponse setUp = await SetUpTest();
        TestResponse before = await _manager.AddJudgersToTestAsync(setUp.Id, 1);
        int answer = before.Judgers[0].Samples[0].Number;
        TestResponse after = await _manager.AddAnswerToTestAsync(setUp.Id, before.Judgers[0].Id, answer);
        JudgerResponse judger = after.Judgers[0];

        Assert.NotNull(judger.Answer);
        Assert.Equal(answer, judger.Answer);
    }

    [Fact]
    public async Task AddAnswerToTestAsync_NullGiven_RemovesAnswer()
    {
        TestResponse setUp = await SetUpTest();
        TestResponse before = await _manager.AddJudgersToTestAsync(setUp.Id, 1);
        int answer = before.Judgers[0].Samples[0].Number;
        await _manager.AddAnswerToTestAsync(setUp.Id, before.Judgers[0].Id, answer);
        TestResponse after = await _manager.AddAnswerToTestAsync(setUp.Id, before.Judgers[0].Id, null);
        
        Assert.Null(after.Judgers[0].Answer);
    }

    #endregion

    #region Helpers

    private async Task<TestResponse> SetUpTest()
    {
        TestAddRequest request = new("test", TestTypes.Triangular, Significances._1);
        return await _manager.AddTestAsync(request);
    }

    private class JudgerResponseComparer : IEqualityComparer<JudgerResponse>
    {
        public bool Equals(JudgerResponse? x, JudgerResponse? y)
        {
            return (x is null, y is null) switch
            {
                (true, true) => true,
                (false, false) => x.Id == y.Id,
                _ => false
            };
        }

        public int GetHashCode([DisallowNull] JudgerResponse obj)
        {
            return obj.GetHashCode();
        }
    }


    #endregion
}