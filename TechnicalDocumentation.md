# Technical documentation

This application is an ASP.NET Core Web App for managing discriminative sensory tests such as triangle and duo-trio tests. It follows the MVC (Model-View-Controller) pattern.

The system allows organizers to:

- Create sensory tests
- Add and remove judgers
- Record judger answers
- Generate results
- Determine statistical relevance

The architecture is designed to support multiple test types through a Strategy Pattern implementation.

Contents:

- [Architecture](#architecture)
- [Domain Model](#domain-model)
- [Persistence](#persistence)
- [Future Improvements](#future-improvements)
- [Contracts](#contracts)

## Architecture

The application is divided into three major layers:

- Controllers
- Services
- Models

Controllers handle the requests; `ITestManagerService` coordinates test operations and persistence. `ITestService` implementations contain business rules specific to each sensory test type.

For example, this is the flow when a user sends a request to add an answer to a judger:

`TestController` validates the request

↓

`ITestManagerService` gets the test's data and passes it to the appropriate service

↓

`ITestServiceFactory` creates the appropriate service for the test type

↓

`TriangularTestService` (for example) checks if the answer is appropriate and adds it

## Domain Model

These are the models:

### Test

Represents a sensory test. Contains its Id, name, test type, significance, etc.

### Judger

Represents a participant in a test. Contains its Id, its test's Id, its samples, and its answer.

### Sample

Represents sensory sample presented to a judger. Contains its Id, its judger's Id, its number, and its type (sample 1 or 2).

## Persistence

Currently tests are stored in Database.json. The file is loaded during application startup and maintained in memory.

Futurely I will migrate to a relational database through Entity Framework Core.

## Future Improvements

- Migration to SQL database
- Pagination support
- Authentication
- Additional sensory test types

## Contracts

These are the contracts for the services:

- ### `ITestManagerService` (Singleton)

This is the main service for managing tests. It reads all tests stored in `Database.json` and stores it in-memory (hence why singleton), also updating the file when needed. Futurely, this will be changed to use a relational database, therefore also being changed to Scoped.

Note: `ITestService.GetTestResponse(Test test)` is used to transform a `Test` object to a `TestResponse` object, as some test types don't expose all the test's data to the user.

It contains the following methods:

- `TestResponse AddTest(TestAddRequest? request)`: Creates a test and returns it with newly generated id.

- `TestResponse AddJudgerToTest(Guid testId)`: Adds a judger in a test and returns the test with the newly added judger.

- `TestResponse? GetTestById(Guid id)`: Gets the test with matching id, or null if there isn't a match.

- `List<TestResponse> GetAllTests()`: Returns all saved tests (to-do: implement pagination).

- `List<JudgerResponse> GetJudgersFromTest(Guid testId)`: Returns only the judgers from a test. This method is only used in unit testing.

- `List<string> GetSamplesInfo(Guid testId)`: Returns a list containing informations about each judger's samples. This is used for displaying.

- `bool DeleteTest(Guid testId)`: Removes a test and returns whether deletion was sucessful.

- `bool RemoveJudgerFromTest(Guid testId, Guid judgerId)`: Removes a judger from a test and returns whether deletion was sucessful.

- `TestResponse AddAnswerToTest(Guid testId, Guid judgerId, Guid? chosenSample)`: Adds a judger's answer and returns the updated test. If `chosenSample` is null, it removes the judger's answer.

- `TestResponse AddAnswerToTest(Guid testId, Guid judgerId, int? chosenSample)`: Same as previous, except that the sample's number (rather than the id) is supplied.

- `TestResult GetTestResults(Guid testId)`: Returns an object containing a test's results. This uses an `ITestService`.

- ### `ITestService`

Each `ITestService` implementation contains the logic for validating, getting results, and getting information about a specific type of test (triangular, duo-trio, etc).

It contains the following methods:

- `bool IsValid(TestAddRequest request)`: Returns whether a test's addition request is suitable for the specific test type.

- `List<Sample> GenerateSamples(Guid? judgerId = null, SampleTypes? differentSample = null)`: Creates samples for a judger. It gives each sample the value of `judgerId` if it's not null, a new `Guid` otherwise. The `differentSample` parameter allows specifying whether `Sample1` or `Sample2` should be the different sample. If it's null, this method chooses randomly.

- `TestResult GetTestResult(Test test)`: Returns the results of a test, including the amount of correct and wrong answers and whether relevance can be asserted.

- `TestResponse GetTestResponse(Test test)`: Transforms a `Test` object to a `TestResponse` one. This method is used by `ITestManagerService` in its CRUD operations. This method exists because some tests don't expose all the test's data to the user and/or judger.

- `string Instructions()`: Returns the instructions for a judger about the test. It's used to generate the record's PDF files.

- `string SamplesInfo()`: Gives information about the samples of the judger to be shown in the PDF to the test's organizer.

- ### `ITestServiceFactory` (Singleton)

Used to create `ITestService` objects to `ITestManager`, as the latter is a singleton and the former are not.

It has only one method:

- `ITestService GetTestService(TestTypes testType)`: Returns an appropriate `ITestService` object for the specific test type.