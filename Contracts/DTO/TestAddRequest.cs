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

    public TestAddRequest(string name, TestTypes testType, Significances significance)
    {
        Name = name;
        Significance = significance;
        TestType = testType;
    }

    public Test ToTest()
    {
        return new(Name, TestType, Significance);
    }
}
