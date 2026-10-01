namespace ereadian.builder.html;

using System.Text;
using ereadian.builder.html.Nodes;
using QRCoder;

public static class Utility
{
    private static readonly Lazy<QRCodeGenerator> QRCodeGeneratorLoader = new(true);

    public static void RenderNodes(
        in IReadOnlyList<IHtmlNode> nodes,
        in StringBuilder builder,
        in IDictionary<string, object> variables)
    {
        foreach (IHtmlNode node in nodes)
        {
            node.Render(builder, variables);
        }
    }

    public static IReadOnlyList<IHtmlNode> LoadNodes(
        HtmlParserContext context,
        string sharedFolder,
        Dictionary<string, HtmlFile> sharedContents)
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
            AddElement(nodes, elementStartPosition, elementNode, context, sharedFolder, sharedContents);
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
        foreach (IHtmlNode node in nodes)
        {
            if (node.NodeType == NodeType.Element)
            {
                elements.Add((ElementNode)node);
            }
        }

        return elements;
    }


    public static void EnsureDirectoryCreated(string? folderFullPath)
    {
        while (!string.IsNullOrEmpty(folderFullPath) && !Directory.Exists(folderFullPath))
        {
            EnsureDirectoryCreated(Path.GetDirectoryName(folderFullPath));
            Directory.CreateDirectory(folderFullPath);
        }
    }

    public static string GenerateQrCode(
        Uri hostUri,
        string sourceRootFolder,
        string targetRootFolder,
        string relativeFolder,
        string fullPath,
        string qrCodeFolderName = "qr-code-img",
        int pixelsPerModule = 20)
    {
        string fileName = Path.GetFileName(fullPath);
        string qrFileName = fileName + ".png";

        string relativeFilePath = Path.Combine(relativeFolder, fileName);
        Uri finalUri = new(hostUri, relativeFilePath);
        string finalUrl = finalUri.ToString();

        string targetFolder = Path.Combine(targetRootFolder, relativeFolder, qrCodeFolderName);
        string targetFullPath = Path.Combine(targetFolder, qrFileName);
        if (!File.Exists(targetFullPath))
        {
            string sourceFolder = Path.Combine(sourceRootFolder, relativeFolder, qrCodeFolderName);
            string sourceFullPath = Path.Combine(sourceFolder, qrFileName);
            if (!File.Exists(sourceFullPath))
            {
                EnsureDirectoryCreated(sourceFolder);

                QRCodeGenerator qrGenerator = QRCodeGeneratorLoader.Value;
                using var qrCodeData = qrGenerator.CreateQrCode(finalUrl, QRCodeGenerator.ECCLevel.Q);
                using var pngQrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeBytes = pngQrCode.GetGraphic(pixelsPerModule);

                File.WriteAllBytes(sourceFullPath, qrCodeBytes);
            }

            EnsureDirectoryCreated(targetFolder);
            File.Copy(sourceFullPath, targetFullPath);
        }

        return Path.Combine(qrCodeFolderName, qrFileName);
    }

    private static void AddElement(
        List<IHtmlNode> nodes,
        int elementStartPosition,
        ElementNode elementNodeToAdd,
        HtmlParserContext context,
        string sharedFolder,
        Dictionary<string, HtmlFile> sharedContents)
    {
        switch (elementNodeToAdd.Name)
        {
            case "variable":
                const string VariableNameAttributeName = "name";
                string variableName = GetAttributeValue(elementNodeToAdd.Attributes, VariableNameAttributeName);
                if (string.IsNullOrEmpty(variableName))
                {
                    throw new InvalidDataException(
                        $"Variable element requires '{VariableNameAttributeName} attribute and the value should not be empty'. File: '{context.FullPath}'.Content:\n{context.Content.Substring(elementStartPosition)}");
                }

                const string VariableValueAttributeName = "value";
                string variableValue = GetAttributeValue(elementNodeToAdd.Attributes, VariableValueAttributeName);
                if (string.IsNullOrEmpty(variableValue))
                {
                    throw new InvalidDataException(
                        $"Variable element requires '{VariableValueAttributeName} attribute and the value should not be empty'. File: '{context.FullPath}'.Content:\n{context.Content.Substring(elementStartPosition)}");
                }

                context.Variables[variableName] = variableValue;
                break;
            case "template":
                const string TemplateNameAttributeName = "name";
                string templateName = GetAttributeValue(elementNodeToAdd.Attributes, TemplateNameAttributeName);
                if (string.IsNullOrEmpty(templateName))
                {
                    throw new InvalidDataException(
                        $"Template element requires '{TemplateNameAttributeName} attribute and the value should not be empty'. File: '{context.FullPath}'.Content:\n{context.Content.Substring(elementStartPosition)}");
                }

                context.Variables[VariableNames.TemplateName] = templateName;
                break;
            case "include":
                const string SharedContentAttributeName = "name";
                string contentName = GetAttributeValue(elementNodeToAdd.Attributes, SharedContentAttributeName);
                if (string.IsNullOrEmpty(contentName))
                {
                    throw new InvalidDataException(
                        $"Include element requires '{SharedContentAttributeName} attribute and the value should not be empty'. File: '{context.FullPath}'.Content:\n{context.Content.Substring(elementStartPosition)}");
                }

                if (!sharedContents.TryGetValue(contentName, out HtmlFile? sharedFile))
                {
                    string sharedContentFullPath = Path.Combine(sharedFolder, $"{contentName}.html");
                    if (!File.Exists(sharedContentFullPath))
                    {
                        return;
                    }

                    sharedFile = new(sharedContentFullPath, string.Empty, context.AllowComment, sharedFolder, sharedContents);
                    sharedContents.Add(contentName, sharedFile);
                }

                context.Variables.Append(sharedFile.Variables);
                ElementNode? htmlNode = sharedFile.Nodes.FirstOrDefault(node => (node.NodeType == NodeType.Element) && ((ElementNode)node).Name == "html") as ElementNode;
                if (htmlNode != null)
                {
                    nodes.AddRange(htmlNode.Children.Where(node => (node.NodeType != NodeType.Element) || ((ElementNode)node).Name != "body"));
                }
                break;
            case BuildActions.BuildElementName:
                string buildTypeName = GetAttributeValue(elementNodeToAdd.Attributes, BuildActions.BuildTypeAttributeName);
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

                DynamicNode dynamicNode = new(elementNodeToAdd, buildAction);
                nodes.Add(dynamicNode);
                break;
            default:
                nodes.Add(elementNodeToAdd);
                break;
        }
    }

    public static T? GetVariableValue<T>(IDictionary<string, object> variables, string key)
    {
        return variables.TryGetValue(key, out object? value) ? (T?)value : default(T);
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