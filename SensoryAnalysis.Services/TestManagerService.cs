using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Services;
/// <summary>
/// The general tests service to store the tasts and do CRUD operations
/// </summary>
public class TestManagerService : ITestManagerService
{
    private readonly List<Test> _tests;
    private readonly ITestServiceFactory _serviceFactory;

    public TestManagerService(ITestServiceFactory serviceFactory)
    {
        _tests = [];
        _serviceFactory = serviceFactory;
    }

    #region Creating

    public TestResponse AddTest(TestAddRequest? request)
    {
        throw new NotImplementedException();
    }

    public TestResponse AddJudgerToTest(Guid testId)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Making the test

    public TestResponse AddAnswerToTest(Guid testId, Guid judgerId, Guid? chosenSample)
    {
        throw new NotImplementedException();
    }

    public TestResponse AddAnswerToTest(Guid testId, Guid judgerId, int? chosenSample)
    {
        throw new NotImplementedException();
    }

    public TestResult GetTestResults(Guid testId)
    {
        throw new NotImplementedException();
    }

    #endregion
}