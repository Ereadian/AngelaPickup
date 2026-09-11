namespace ereadian.builder.html;

using System.Text;
using ereadian.builder.html.Nodes;

public static class Utility
{
    public static void RenderNodes(
        in IReadOnlyList<IHtmlNode> nodes,
        in StringBuilder builder,
        in IDictionary<string, object> variables)
    {
        foreach(IHtmlNode node in nodes)
        {
            node.Render(builder, variables);
        }
    }

    public static IReadOnlyList<IHtmlNode> LoadNodes(
        HtmlParserContext context,
        string sharedFolder,
        Dictionary<string, List<IHtmlNode>> sharedContents)
    {
        List<IHtmlNode> nodes = [];
        while (!context.IsEnd())
        {
            int tagStart = context.Content.IndexOf('<', context.CurrentPosition);
            if (tagStart < 0)
            {
                tagStart = context.Content.Length;
            }

            int length = tagStart - context.CurrentPosition;
            if (length > 0)
            {
                LiteratureNode node = new LiteratureNode(context.Content.Substring(context.CurrentPosition, length));
                nodes.Add(node);
            }

            context.CurrentPosition = tagStart;
            if (context.IsEnd())
            {
                break;
            }

            if (context.StartsWith("<!"))
            {
                const string CDataOpenTag = "<![CDATA[";
                const string CDataCloseTag = "]]>";
                const string DocTypeOpenTag = "<!DOCTYPE";
                const string CommentOpenTag = "<!--";
                const string CommentCloseTag = "-->";
                if (context.StartsWith(CDataOpenTag))
                {
                    nodes.Add(CreateMarkNode(context, CDataOpenTag, CDataCloseTag));
                }
                else if (context.StartsWith(DocTypeOpenTag))
                {
                    nodes.Add(CreateMarkNode(context, DocTypeOpenTag, ">"));
                }
                else if (context.StartsWith(CommentOpenTag))
                {
                    LiteratureNode node = CreateMarkNode(context, CommentOpenTag, CommentCloseTag);
                    if (context.AllowComment)
                    {
                        nodes.Add(node);
                    }
                }
                else
                {
                    throw new InvalidDataException(
                        $"Unknown html special tag. File: '{context.FullPath}'.Content:\n{context.Content.Substring(context.CurrentPosition)}");
                }

                continue;
            }

            if (context.StartsWith("</"))
            {
                break;
            }

            int elementStartPosition = context.CurrentPosition;
            ElementNode elementNode = ElementNode.Parse(context, sharedFolder, sharedContents);
            switch(elementNode.Name)
            {
                case "variable":
                    const string VariableNameAttributeName = "name";
                    string variableName = GetAttributeValue(elementNode.Attributes, VariableNameAttributeName);
                    if (string.IsNullOrEmpty(variableName))
                    {
                        throw new InvalidDataException(
                            $"Variable element requires '{VariableNameAttributeName} attribute and the value should not be empty'. File: '{context.FullPath}'.Content:\n{context.Content.Substring(elementStartPosition)}");
                    }

                    const string VariableValueAttributeName = "value";
                    string variableValue = GetAttributeValue(elementNode.Attributes, VariableValueAttributeName);
                    if (string.IsNullOrEmpty(variableValue))
                    {
                        throw new InvalidDataException(
                            $"Variable element requires '{VariableValueAttributeName} attribute and the value should not be empty'. File: '{context.FullPath}'.Content:\n{context.Content.Substring(elementStartPosition)}");
                    }

                    context.Variables[variableName] = variableValue;
                    break;
                case "template":
                    const string TemplateNameAttributeName = "name";
                    string templateName = GetAttributeValue(elementNode.Attributes, TemplateNameAttributeName);
                    if (string.IsNullOrEmpty(templateName))
                    {
                        throw new InvalidDataException(
                            $"Template element requires '{TemplateNameAttributeName} attribute and the value should not be empty'. File: '{context.FullPath}'.Content:\n{context.Content.Substring(elementStartPosition)}");
                    }

                    context.Variables[VariableNames.TemplateName] = templateName;
                    break;
                case "include":
                    const string SharedContentAttributeName = "name";
                    string contentName = GetAttributeValue(elementNode.Attributes, SharedContentAttributeName);
                    if (string.IsNullOrEmpty(contentName))
                    {
                        throw new InvalidDataException(
                            $"Include element requires '{SharedContentAttributeName} attribute and the value should not be empty'. File: '{context.FullPath}'.Content:\n{context.Content.Substring(elementStartPosition)}");
                    }

                    if (!sharedContents.TryGetValue(contentName, out List<IHtmlNode>? contents))
                    {
                        string sharedContentFullPath = Path.Combine(sharedFolder, $"{contentName}.html");
                        if (!File.Exists(sharedContentFullPath))
                        {
                            contents = [];
                        }
                        else
                        {
                            HtmlFile file = new (sharedContentFullPath, string.Empty, context.AllowComment, sharedFolder, sharedContents);
                            contents = [.. file.Nodes];
                        }

                        sharedContents.Add(contentName, contents);
                    }

                    nodes.AddRange(contents);
                    break;
                case BuildActions.BuildElementName:
                    string buildTypeName = GetAttributeValue(elementNode.Attributes, BuildActions.BuildTypeAttributeName);
                    if (string.IsNullOrEmpty(buildTypeName))
                    {
                        throw new InvalidDataException(
                            $"Build element requires '{BuildActions.BuildTypeAttributeName} attribute and the value should not be empty'. File: '{context.FullPath}'.Content:\n{context.Content.Substring(elementStartPosition)}");
                    }

                    if (!BuildActions.Actions.TryGetValue(buildTypeName, out var buildAction))
                    {
                        throw new InvalidDataException(
                            $"Unknown build action name '{buildTypeName}'. File: '{context.FullPath}'.Content:\n{context.Content.Substring(elementStartPosition)}");
                    }

                    DynamicNode dynamicNode = new (elementNode, buildAction);
                    nodes.Add(dynamicNode);
                    break;
                default:
                    nodes.Add(elementNode);
                    break;
            }
        }

        return nodes;
    }

    public static string GetAttributeValue(IReadOnlyList<AttributeNode> attributeNodes, string name)
    {
        return attributeNodes.FirstOrDefault(attribute => attribute.Name == name)?.Value ?? string.Empty;
    }

    public static IReadOnlyList<ElementNode> GetElementNodes(IReadOnlyList<IHtmlNode> nodes)
    {
        List<ElementNode> elements = new(nodes.Count);
        foreach(IHtmlNode node in nodes)
        {
            if (node.NodeType == NodeType.Element)
            {
                elements.Add((ElementNode)node);
            }
        }

        return elements;
    }

    private static LiteratureNode CreateMarkNode(HtmlParserContext context, string openTag, string closeTag)
    {
        int endTagPosition = context.Content.IndexOf(closeTag, context.CurrentPosition + openTag.Length);
        if (endTagPosition < 0)
        {
            throw new InvalidDataException(
                $"Incomplete segment for '{openTag}'. File: '{context.FullPath}'.Content:\n{context.Content.Substring(context.CurrentPosition)}");
        }

        int endPosition = endTagPosition + closeTag.Length;
        string data = context.Content.Substring(context.CurrentPosition, endPosition - context.CurrentPosition);
        context.CurrentPosition = endPosition;
        return new LiteratureNode(data);
    }
}