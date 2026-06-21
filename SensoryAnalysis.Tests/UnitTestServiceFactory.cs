using Microsoft.Extensions.Logging;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Entities;
using SensoryAnalysis.Services;

namespace SensoryAnalysis.Tests;
internal class UnitTestServiceFactory : ITestServiceFactory
{
    public ITestService GetTestService(TestTypes testType)
    {
        return testType switch
        {
            TestTypes.Triangular => new TriangularTestService(new Logger<TriangularTestService>(new LoggerFactory())),
            TestTypes.DuoTrio => new DuoTrioTestService(new Logger<DuoTrioTestService>(new LoggerFactory())),
            _ => throw new NotImplementedException($"This method cannot yet provide a new {testType}'s service object"),
        };
    }
}