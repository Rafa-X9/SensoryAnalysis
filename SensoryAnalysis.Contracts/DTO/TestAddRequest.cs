using SensoryAnalysis.Entities;
using System.ComponentModel.DataAnnotations;

namespace SensoryAnalysis.Contracts.DTO;
public class TestAddRequest
{
    [Required(ErrorMessage = "Tests must have a name")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Tests must have a test type")]
    public TestTypes TestType { get; set; }

    [Required(ErrorMessage = "Tests must have the sensitivity")]
    public Significances Significance { get; set; }

    public string? NameOfSample1 { get; set; }
    public string? NameOfSample2 { get; set; }

    public TestAddRequest()
    {
        Name = string.Empty;
        TestType = TestTypes.Triangular;
        Significance = Significances._5;
    }

    public TestAddRequest(string name,
        TestTypes testType,
        Significances significance,
        string? nameOfSample1 = null,
        string? nameOfSample2 = null)
    {
        Name = name;
        Significance = significance;
        TestType = testType;
        NameOfSample1 = nameOfSample1;
        NameOfSample2 = nameOfSample2;
    }

    public Test ToTest()
    {
        return new(Name,
            TestType,
            Significance,
            nameOfSample1: NameOfSample1,
            nameOfSample2: NameOfSample2);
    }
}
