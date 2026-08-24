namespace ereadian.builder.html;

using System.Globalization;

public static class BuildFunctions
{
    public static IReadOnlyDictionary<string, Func<string, string, IDictionary<string, object>, CultureInfo, string>> Functions
        => new Dictionary<string, Func<string, string, IDictionary<string, object>, CultureInfo, string>>()
        {
            {VariableNames.SiteBuildTime, RenderSiteBuildTime},
        };

    public static string RenderSiteBuildTime(string name, string data, IDictionary<string, object> variables, CultureInfo siteCulture)
    {
        DateTime siteBuildTime = (DateTime)variables[VariableNames.SiteBuildTime];
        return siteBuildTime.ToString(siteCulture);
    }
}
