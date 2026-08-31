namespace ereadian.builder.html.Nodes;

using System.Text;

public class LiteratureNode(string text) : IHtmlNode
{
    public NodeType NodeType => NodeType.Literature;

    public void Render(in StringBuilder builder, in IDictionary<string, object> variables)
    {
        _ = builder.Append(text);
    }
}