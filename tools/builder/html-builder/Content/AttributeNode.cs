namespace ereadian.builder.html.Content;

using System.Text;

public class AttributeNode(string name,string data)
    : IHtmlNode
{
    private string? value = null;

    public string Value
    {
        get
        {
            if (this.value is null)
            {
                this.value = data[1..^1];
            }

            return this.value;
        }
    }

    public void Render(in StringBuilder builder, in IDictionary<string, object> variables)
    {
        _ = builder.AppendFormat("{0}={1}", name, data);
    }
}