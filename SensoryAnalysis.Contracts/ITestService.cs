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
    /// <param name="judgerId">
    /// The judger Id to add to the sample. If it's null, a new Guid is created
    /// </param>
    /// <param name="differentSample">
    /// If a value is given, the different sample (i.e. the correct one) will be
    /// of the given type
    /// </param>
    /// <returns>List containing newly generated samples</returns>
    List<Sample> GenerateSamples(Guid? judgerId = null, SampleTypes ? differentSample = null);

    /// <summary>
    /// Generates a test's results
    /// </summary>
    /// <returns>A <see cref="TestResult"/> object containing the test's results</returns>
    /// <exception cref="ArgumentException">Thrown if the given test object is not of the service's test type</exception>
    TestResult GetTestResult(Test test);

    /// <summary>
    /// Turns a test to test response. This method exists because some tests don't
    /// show all the judger's information to the judger.
    /// </summary>
    /// <param name="test">The test to get the response from</param>
    /// <returns>The test response</returns>
    TestResponse GetTestResponse(Test test);

    /// <summary>
    /// Gives the instructions to the judger about the specific test
    /// </summary>
    /// <returns>String containing the instructions</returns>
    string Instructions();

    /// <summary>
    /// Gives information about the samples of the judger to be shown in
    /// the PDF which will contain the numbers to print and put next to
    /// the samples in the test
    /// </summary>
    /// <param name="judger">The test to get info about</param>
    /// <param name="test">The test the judger is in</param>
    /// <returns>Information about the arrangement of samples</returns>
    /// <exception cref="ArgumentException">Thrown if an invalid test/judger is given</exception>
    string SamplesInfo(Judger judger, Test test);
}