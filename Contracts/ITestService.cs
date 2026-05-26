using SensoryAnalysis.Contracts.DTO;
using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Contracts;
/// <summary>
/// Test service to validate and get the results of a test.
/// This interface will have an implementation for each test type.
/// </summary>
public interface ITestService
{
    /// <summary>
    /// Returns whether the request object is a valid test for the service's test type
    /// </summary>
    /// <param name="request">The request object to validate</param>
    /// <returns>Boolean representing if the request object is valid or not for the specific test type</returns>
    /// <exception cref="ArgumentException">Thrown if the given test object is not of the service's test type</exception>
    bool IsValid(TestAddRequest request);

    /// <summary>
    ///  Generates samples for a judger
    /// </summary>
    /// <param name="differentSample">
    /// If a value is given, the different sample (i.e. the correct one) will be
    /// of the given type
    /// </param>
    /// <returns>List containing newly generated samples</returns>
    List<Sample> GenerateSamples(SampleTypes? differentSample = null);

    /// <summary>
    /// Generates a test's results
    /// </summary>
    /// <returns>A <see cref="TestResult"/> object containing the test's results</returns>
    /// <exception cref="ArgumentException">Thrown if the given test object is not of the service's test type</exception>
    TestResult GetTestResult(Test test);
}