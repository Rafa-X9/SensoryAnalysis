using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SensoryAnalysis.Contracts;
using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Services;
/// <summary>
/// Service to provide the appropriate <see cref="ITestService"/> object
/// </summary>
public class TestServiceFactory : ITestServiceFactory
{
    private readonly IServiceProvider _serviceprovider;

    public TestServiceFactory(IServiceProvider serviceprovider)
    {
        _serviceprovider = serviceprovider;
    }

    public ITestService GetTestService(TestTypes testType)
    {
        return testType switch
        {
            TestTypes.Triangular => new TriangularTestService(_serviceprovider.GetRequiredService<ILogger<TriangularTestService>>()),
            TestTypes.DuoTrio => new DuoTrioTestService(_serviceprovider.GetRequiredService<ILogger<DuoTrioTestService>>()),
            _ => throw new NotImplementedException($"This method cannot yet provide a new {testType}'s service object"),
        };
    }
}