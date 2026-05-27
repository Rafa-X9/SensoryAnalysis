namespace SensoryAnalysis.Services.Helpers;
public static class ExtensionHelpers
{
    public static bool IsIn<T>(this T obj, params T[] array)
    {
        return array.Contains(obj);
    }
}