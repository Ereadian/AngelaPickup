namespace ereadian.builder.html;

using System.Globalization;
using System.Text;
using ereadian.builder.html.Nodes;

public static class BuildActions
{
    public static IReadOnlyDictionary<string, Action<ElementNode, StringBuilder, IDictionary<string, object>>> Actions
        => new Dictionary<string, Action<ElementNode, StringBuilder, IDictionary<string, object>>>()
        {
            {VariableNames.SiteBuildTime, RenderSiteBuildTime},
        };

    public static void RenderSiteBuildTime(
        ElementNode elementNode,
        StringBuilder builder,
        IDictionary<string, object> variables)
    {
        DateTime siteBuildTime = (DateTime)variables[VariableNames.SiteBuildTime];
        string value = siteBuildTime.ToString(variables[VariableNames.SiteCulture] as CultureInfo);
        _ = builder.Append(value);
    }
}
