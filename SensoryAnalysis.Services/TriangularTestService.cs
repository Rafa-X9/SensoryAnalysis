using SensoryAnalysis.Contracts;
using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Services;

/// <summary>
/// The <see cref="ITestService"/> implementation for triangular tests
/// </summary>
public class TriangularTestService : ITestService
{
    public bool IsValid(TestAddRequest request)
    {
        throw new NotImplementedException();
    }

    public List<Sample> GenerateSamples(SampleTypes? differentSample = null)
    {
        throw new NotImplementedException();
    }

    public TestResult GetTestResult(Test test)
    {
        throw new NotImplementedException();
    }
}