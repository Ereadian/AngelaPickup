namespace ereadian.builder.html.Nodes;

using System.Text;

public class LiteratureNode(string data) : IHtmlNode
{
    public NodeType NodeType => NodeType.Literature;

    public string Content => data;

    public void Render(in StringBuilder builder, in IDictionary<string, object> variables)
    {
        _ = builder.Append(data);
    }
}