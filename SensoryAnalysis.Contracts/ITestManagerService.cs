using SensoryAnalysis.Contracts.DTO;

namespace SensoryAnalysis.Contracts;
/// <summary>
/// Interface to make the general test services.
/// This interface will handle the CRUD operations of tests,
/// passing them to the appropriate service
/// </summary>
public interface ITestManagerService
{
    #region Creating

    /// <summary>
    /// Creates a test
    /// </summary>
    /// <param name="request">The test to add</param>
    /// <returns>A <see cref="TestResponse"/> object containing the newly created test</returns>
    /// <exception cref="ArgumentNullException">Thrown if null is passed as argumemt</exception>
    /// <exception cref="ArgumentException">
    /// Thrown if the specific test service deems the add request as unproper
    /// </exception>
    TestResponse AddTest(TestAddRequest? request);

    /// <summary>
    /// Adds a judger to the specified test. This test also generates the samples
    /// for the newly added judger
    /// </summary>
    /// <param name="testId">The id of the test to which the judger will be added</param>
    /// <returns>A <see cref="TestResponse"/> object containing the updated test info</returns>
    /// <exception cref="ArgumentException">Thrown if no matching Id is found</exception>
    TestResponse AddJudgerToTest(Guid testId);

    #endregion

    #region Reading

    /// <summary>
    /// Returns the test with the searched Id, or null if there is none
    /// </summary>
    /// <param name="id">The Id to search for</param>
    /// <returns>The respective test, or null if there are no matches</returns>
    TestResponse? GetTestById(Guid id);

    /// <summary>
    /// Returns all tests
    /// </summary>
    /// <returns>List containing all tests</returns>
    List<TestResponse> GetAllTests();

    /// <summary>
    /// Returns all judgers in the given test
    /// </summary>
    /// <param name="testId">The test's Id to search for</param>
    /// <returns>List containing all judgers from the test</returns>
    /// <exception cref="ArgumentException">Thrown if the given Id has no matches</exception>
    List<JudgerResponse> GetJudgersFromTest(Guid testId);

    #endregion

    #region Removing

    /// <summary>
    /// Deletes a test and returns whether if it was sucessful
    /// </summary>
    /// <param name="testId">The test to delete's Id</param>
    /// <returns>True if sucessful, false otherwise</returns>
    bool DeleteTest(Guid testId);

    #endregion

    #region Making the test

    /// <summary>
    /// Adds the sample a judger chose in a test
    /// </summary>
    /// <param name="testId">The test to add an answer to a judge in</param>
    /// <param name="judgerId">The judger to add a response to</param>
    /// <param name="chosenSample">The Id of the sample the judger chose</param>
    /// <returns>A <see cref="TestResponse"/> object with updated data, null to remove an answer</returns>
    TestResponse AddAnswerToTest(Guid testId, Guid judgerId, Guid? chosenSample);

    /// <summary>
    /// Adds the sample a judger chose in a test
    /// </summary>
    /// <param name="testId">The test to add an answer to a judge in</param>
    /// <param name="judgerId">The judger to add a response to</param>
    /// <param name="chosenSample">The number of the sample the judger chose</param>
    /// <returns>A <see cref="TestResponse"/> object with updated data, null to remove an answer</returns>
    TestResponse AddAnswerToTest(Guid testId, Guid judgerId, int? chosenSample);

    /// <summary>
    /// Gets a test's results
    /// </summary>
    /// <param name="testId">The test to get the results of</param>
    /// <returns>A <see cref="TestResult"/> object containing the test's result data</returns>
    /// <exception cref="ArgumentException">Thrown if there's no matching Id</exception>
    TestResult GetTestResults(Guid testId);

    #endregion
}