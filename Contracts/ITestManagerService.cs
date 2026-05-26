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

    #region Making the test

    /// <summary>
    /// Adds the sample a judger chose in a test
    /// </summary>
    /// <param name="testId">The test to add an answer to a judge in</param>
    /// <param name="judgerId">The judger to add a response to</param>
    /// <param name="chosenSample">The sample the judger chose</param>
    /// <returns>A <see cref="TestResponse"/> object with updated data</returns>
    TestResponse AddAnswerToTest(Guid testId, Guid judgerId, Guid chosenSample);

    #endregion
}