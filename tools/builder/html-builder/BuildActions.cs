namespace ereadian.builder.html;

using System.Globalization;
using System.Text;
using ereadian.builder.html.Nodes;

public static class BuildActions
{
    public const string BuildElementName = "build";
    public const string BuildTypeAttributeName = "type";

    public static IReadOnlyDictionary<string, Action<ElementNode, StringBuilder, IDictionary<string, object>>> Actions
        => new Dictionary<string, Action<ElementNode, StringBuilder, IDictionary<string, object>>>()
        {
            {nameof(RenderSiteBuildTime), RenderSiteBuildTime},
            {nameof(RenderDateTime), RenderDateTime},
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
    /// Render date time where the data is stored in variable collection.
    /// </summary>
    /// <param name="elementNode">the element contains the details.</param>
    /// <param name="builder">the string builder.</param>
    /// <param name="variables">the render variables.</param>
    /// <example>
    /// <![CDATA[
    /// <build type="RenderDateTime" name='current-file-modified-time' />
    /// ]]>
    /// </example>
    public static void RenderDateTime(
        ElementNode elementNode,
        StringBuilder builder,
        IDictionary<string, object> variables)
    {
        string variableName = Utility.GetAttributeValue(elementNode.Attributes, "name").Trim();
        if ((variableName.Length < 1) || !variables.TryGetValue(variableName, out object? value))
        {
            return;
        }

        DateTime time = (DateTime)value;
        CultureInfo siteCultureInfo = (CultureInfo)variables[VariableNames.SiteCulture];
        _ = builder.Append(time.ToString(siteCultureInfo));
    }

    /// <summary>
    /// Inject Html Element.
    /// </summary>
    /// <param name="elementNode">the element contains the details.</param>
    /// <param name="builder">the string builder.</param>
    /// <param name="variables">the render variables.</param>
    /// <example>
    /// <![CDATA[
    /// <build type = "InjectHtmlElement" tag="header" />
    /// <build type = "InjectHtmlElement" tag="content" name="foot" />
    /// ]]>
    /// </example>
    public static void InjectHtmlElement(
        ElementNode elementNode,
        StringBuilder builder,
        IDictionary<string, object> variables)
    {
        string elementTag = Utility.GetAttributeValue(elementNode.Attributes, "tag").Trim();
        if (string.IsNullOrEmpty(elementTag))
        {
            return;
        }

        string elementName = Utility.GetAttributeValue(elementNode.Attributes, "name").Trim();

        if (variables.TryGetValue(VariableNames.RootElementsToInject, out object? rowMapping))
        {
            Dictionary<string, List<ElementNode>> elementMapping = (Dictionary<string, List<ElementNode>>)rowMapping;
            if (elementMapping.TryGetValue(elementTag, out List<ElementNode>? elements))
            {
                foreach(ElementNode element in elements)
                {
                    if ((elementName.Length < 1) || (elementName == Utility.GetAttributeValue(element.Attributes, "name").Trim()))
                    {
                        Utility.RenderNodes(element.Children, builder, variables);
                    }
                }
            }
        }
    }
}
