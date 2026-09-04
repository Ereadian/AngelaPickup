namespace ereadian.builder.html;

using System.Globalization;
using System.Text;
using ereadian.builder.html.Nodes;

public static class BuildActions
{
    public const string BuildTypeAttributeName = "type";

    public static IReadOnlyDictionary<string, Action<ElementNode, StringBuilder, IDictionary<string, object>>> Actions
        => new Dictionary<string, Action<ElementNode, StringBuilder, IDictionary<string, object>>>()
        {
            {nameof(RenderSiteBuildTime), RenderSiteBuildTime},
            {nameof(InjectHtmlElement), InjectHtmlElement},
        };

    /// <summary>
    /// Render site build time.
    /// </summary>
    /// <param name="elementNode">the element contains the details.</param>
    /// <param name="builder">the string builder.</param>
    /// <param name="variables">the render variables.</param>
    /// <example>
    /// <![CDATA[
    /// <build type = "RenderSiteBuildTime" />
    /// ]]>
    /// </example>
    public static void RenderSiteBuildTime(
        ElementNode elementNode,
        StringBuilder builder,
        IDictionary<string, object> variables)
    {
        DateTime siteBuildTime = (DateTime)variables[VariableNames.SiteBuildTime];
        string value = siteBuildTime.ToString(variables[VariableNames.SiteCulture] as CultureInfo);
        _ = builder.Append(value);
    }

    /// <summary>
    /// Inject Html Element.
    /// </summary>
    /// <param name="elementNode">the element contains the details.</param>
    /// <param name="builder">the string builder.</param>
    /// <param name="variables">the render variables.</param>
    /// <example>
    /// <![CDATA[
    /// <build type = "InjectHtmlElement" name="body" />
    /// ]]>
    /// </example>
    public static void InjectHtmlElement(
        ElementNode elementNode,
        StringBuilder builder,
        IDictionary<string, object> variables)
    {
        string? elementName = Utility.GetAttributeValue(elementNode.Attributes, "name");
        if (string.IsNullOrEmpty(elementName?.Trim()))
        {
            return;
        }

        if (variables.TryGetValue(VariableNames.RootElementsToInject, out object? rowMapping))
        {
            Dictionary<string, ElementNode> elementMapping = (Dictionary<string, ElementNode>)rowMapping;
            if (elementMapping.TryGetValue(elementName, out ElementNode? element))
            {
                Utility.RenderNodes(element.Children, builder, variables);
            }
        }
    }
}
