using System.ComponentModel.DataAnnotations;

namespace SensoryAnalysis.Services.Helpers;
internal static class ValidatorHelper
{
    internal static void ValidateObject(object obj)
    {
        if (!Validator.TryValidateObject(obj, new(obj), [], true))
        {
            throw new ArgumentException("Invalid object");
        }
    }
}