namespace ereadian.builder.html.Content;

using System.Text;

public class DynamicContent(
    string name,
    string data,
    Func<string, string, IDictionary<string, object>, string> buildFunction) : IHtmlNode
{
    public NodeType NodeType => NodeType.Dynamic;

    public void Render(in StringBuilder builder, in IDictionary<string, object> variables)
    {
        object content = buildFunction.Invoke(name, data, variables);
        _ = builder.Append(content);
    }
}