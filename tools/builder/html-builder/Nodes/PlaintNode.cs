namespace ereadian.builder.html.Nodes;

using System.Text;

public class PlaintNode(string text) : IHtmlNode
{
    public NodeType NodeType => NodeType.Text;

    public void Render(in StringBuilder builder, in IDictionary<string, object> variables)
    {
        _ = builder.Append(text);
    }
}