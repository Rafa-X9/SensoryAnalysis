using SensoryAnalysis.Contracts;
using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Services;
/// <summary>
/// Service to provide the appropriate <see cref="ITestService"/> object
/// </summary>
public class TestServiceFactory : ITestServiceFactory
{
    public ITestService GetTestService(TestTypes testType)
    {
        return testType switch
        {
            TestTypes.Triangular => new TriangularTestService(),
            TestTypes.DuoTrio => new DuoTrioTestService(),
            _ => throw new NotImplementedException($"This method cannot yet provide a new {testType}'s service object"),
        };
    }
}