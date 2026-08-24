namespace ereadian.builder.html;

using System.Globalization;

public static class BuildFunctions
{
    public static IReadOnlyDictionary<string, Func<string, string, IDictionary<string, object>, string>> Functions
        => new Dictionary<string, Func<string, string, IDictionary<string, object>, string>>()
        {
            {VariableNames.SiteBuildTime, RenderSiteBuildTime},
        };

    public static string RenderSiteBuildTime(string name, string data, IDictionary<string, object> variables)
    {
        DateTime siteBuildTime = (DateTime)variables[VariableNames.SiteBuildTime];
        return siteBuildTime.ToString(variables[VariableNames.SiteCulture] as CultureInfo);
    }
}
