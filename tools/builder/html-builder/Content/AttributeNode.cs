namespace ereadian.builder.html.Content;

using System.Text;
using System.Web;

public class AttributeNode(string name,string data)
    : IHtmlNode
{
    private string? value = null;

    public NodeType NodeType => NodeType.Attribute;
    public string Name => name;
    public string Data => data;
    public string Value
    {
        get
        {
            this.value ??= HttpUtility.HtmlDecode(data[1..^1]);
            return this.value;
        }
    }

    public void Render(in StringBuilder builder, in IDictionary<string, object> variables)
    {
        _ = builder.AppendFormat("{0}={1}", name, data);
    }
}