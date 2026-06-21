using SensoryAnalysis.Entities;

namespace SensoryAnalysis.Services.Helpers;
public static class LogicExtensionHelpers
{
    public static bool IsIn<T>(this T obj, params T[] array)
    {
        return array.Contains(obj);
    }

    public static SampleTypes OtherSampleType(this SampleTypes type)
    {
        return type == SampleTypes.Sample1 ? SampleTypes.Sample2 : SampleTypes.Sample1;
    }
}