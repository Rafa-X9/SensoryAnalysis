using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Contracts;
public interface ITestServiceFactory
{
    /// <summary>
    /// Returns the appropriate <see cref="ITestService"/> implementation for that test type
    /// </summary>
    /// <param name="testType">The test type to get a service for</param>
    /// <returns>A <see cref="ITestService"/> service</returns>
    ITestService GetTestService(TestTypes testType);
}