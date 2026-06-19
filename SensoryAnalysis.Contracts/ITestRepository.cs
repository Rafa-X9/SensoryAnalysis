using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Contracts;
/// <summary>
/// Contract for the methods a Test Repository should have
/// </summary>
public interface ITestRepository
{
    /// <summary>
    /// Gets all saved tests.
    /// </summary>
    /// <returns>List containing all tests.</returns>
    Task<List<Test>> GetAllTestsAsync();

    /// <summary>
    /// Gets a test by its id, or null if there are no tests.
    /// </summary>
    /// <param name="id">The id to search for</param>
    /// <returns></returns>
    Task<Test?> GetTestByIdAsync(Guid id);

    /// <summary>
    /// Adds the test in the database and returns it.
    /// </summary>
    /// <param name="test">The test to add</param>
    /// <returns>The same test</returns>
    Task<Test> AddTestAsync(Test test);

    /// <summary>
    /// Adds the judger to the matching test
    /// </summary>
    /// <param name="judger">The judger to add</param>
    /// <param name="testId">The test to add the judger to</param>
    /// <returns>The updated test</returns>
    Task<Test> AddJudgerToTestAsync(Judger judger, Guid testId);

    /// <summary>
    /// Removes a test from the database
    /// </summary>
    /// <param name="testId">The id to remove</param>
    /// <returns>Returns whether deletion was sucessful or not</returns>
    Task<bool> DeleteTestAsync(Guid testId);

    /// <summary>
    /// Removes a judger
    /// </summary>
    /// <param name="judgerId">The judger to remove</param>
    /// <returns>Returns whether deletion was sucessful or not</returns>
    Task<bool> RemoveJudgerFromTestAsync(Guid judgerId);

    /// <summary>
    /// Adds a judger's answer to a test
    /// </summary>
    /// <param name="judgerId">The judger to add an answer to</param>
    /// <param name="chosenSample">The sample the judger chose. Null removes the answer</param>
    /// <returns>The updated test</returns>
    Task<Test> AddAnswerToTestAsync(Guid judgerId, Guid? chosenSample);

    /// <summary>
    /// Adds a judger's answer to a test
    /// </summary>
    /// <param name="judgerId">The judger to add an answer to</param>
    /// <param name="chosenSample">The sample the judger chose. Null removes the answer</param>
    /// <returns>The updated test</returns>
    Task<Test> AddAnswerToTestAsync(Guid judgerId, int? chosenSample);
}