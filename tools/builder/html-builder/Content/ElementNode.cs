namespace ereadian.builder.html.Content;

using System.Text;

public class ElementNode(
    string name,
    IReadOnlyList<AttributeNode> attributes,
    IReadOnlyList<IHtmlNode> children,
    bool isShortElement)
    : IHtmlNode
{
    public NodeType NodeType => NodeType.Element;

    public string Name => name;
    public IReadOnlyList<AttributeNode> Attributes => attributes;
    public IReadOnlyList<IHtmlNode> Children => children;

    public static ElementNode Parse(HtmlParserContext context)
    {
        if (context.GetCurrentChar() != '<')
        {
            throw new InvalidDataException(
                $"Element should start with '<' but it is not. File:'{context.FullPath}. Content:\n{context.Content.Substring(context.CurrentPosition)}");
        }

        int elementStartPosition = context.CurrentPosition;
        context.CurrentPosition++;
        context.SkipWhiteSpace();
        string tagName = context.GetName();
        if (tagName.Length < 1)
        {
            throw new InvalidDataException(
                $"Element should have tag name. File:'{context.FullPath}. Content:\n{context.Content[elementStartPosition..]}");
        }

        context.CurrentPosition += tagName.Length;

        List<AttributeNode> attributes = [];
        while (true)
        {
            context.SkipWhiteSpace();
            string attributeName = context.GetName();
            if (attributeName.Length < 1)
            {
                break;
            }

            context.CurrentPosition += attributeName.Length;
            context.SkipWhiteSpace();
            if (context.GetCurrentChar() != '=')
            {
                throw new InvalidDataException(
                    $"Attribute should have '=' followed but it does not. File:'{context.FullPath}. Content:\n{context.Content[elementStartPosition..]}");
            }

            context.CurrentPosition++;
            context.SkipWhiteSpace();
            char quotationMark = context.GetCurrentChar();
            if ((quotationMark != '\'') && (quotationMark != '"'))
            {
                throw new InvalidDataException(
                    $"Attribute value should start with quotation mark but it does not. File:'{context.FullPath}. Content:\n{context.Content[elementStartPosition..]}");
            }

            int quotationMarkEndPosition = context.Content.IndexOf(quotationMark, context.CurrentPosition + 1);
            if (quotationMarkEndPosition < 1)
            {
                throw new InvalidDataException(
                    $"Attribute value should close by quotation mark {quotationMark} but it does not. File:'{context.FullPath}. Content:\n{context.Content[context.CurrentPosition..]}");
            }

            quotationMarkEndPosition++;
            string data = context.Content.Substring(context.CurrentPosition, quotationMarkEndPosition - context.CurrentPosition);
            context.CurrentPosition = quotationMarkEndPosition;
            AttributeNode attributeNode = new (attributeName, data);
            attributes.Add(attributeNode);
        }

        context.SkipWhiteSpace();        
        const string ElementShortCloseTag = "/>";
        if (context.StartsWith(ElementShortCloseTag))
        {
            context.CurrentPosition += ElementShortCloseTag.Length;
            return new ElementNode(tagName, attributes, [], true);
        }

        if (context.IsEnd() || (context.GetCurrentChar() != '>'))
        {
            throw new InvalidDataException(
                $"Expect end element tag '>' but it does not. File:'{context.FullPath}. Content:\n{context.Content[context.CurrentPosition..]}");
        }

        context.CurrentPosition++;
        IReadOnlyList<IHtmlNode> children = Utility.LoadNodes(context);

        context.SkipWhiteSpace();
        const string ElementCloseTag = "</";
        if (context.IsEnd() || !context.StartsWith(ElementShortCloseTag))
        {
            throw new InvalidDataException(
                $"Expect end element close tag '{ElementCloseTag}' but it does not. File:'{context.FullPath}. Content:\n{context.Content[context.CurrentPosition..]}");
        }

        context.CurrentPosition += ElementShortCloseTag.Length;
        context.SkipWhiteSpace();
        string endTagName = context.GetName();
        if (endTagName != tagName)
        {
            throw new InvalidDataException(
                $"Element close tag should be '{tagName}' but it is '{endTagName}'. File:'{context.FullPath}. Content:\n{context.Content[context.CurrentPosition..]}");
        }

        context.CurrentPosition += endTagName.Length;
        context.SkipWhiteSpace();
        if (context.IsEnd() || context.GetCurrentChar() != '>')
        {
            throw new InvalidDataException(
                $"Element close tag should end by '>' but it is not. File:'{context.FullPath}. Content:\n{context.Content[context.CurrentPosition..]}");
        }

        context.CurrentPosition++;
        return new ElementNode(tagName, attributes, children, false);
    }

    public void Render(in StringBuilder builder, in IDictionary<string, object> variables)
    {
        _ = builder.Append('<');
        _ = builder.Append(this.Name);
        foreach (AttributeNode child in this.Attributes)
        {
            _ = builder.Append(' ');
            child.Render(builder, variables);
        }

        if (isShortElement && (children.Count < 1))
        {
            _ = builder.Append("/>");
            return;
        }

        _ = builder.Append('>');
        Utility.RenderNodes(children, builder, variables);
        _ = builder.Append("</");
        _ = builder.Append(this.Name);
        _ = builder.Append('>');
    }
}