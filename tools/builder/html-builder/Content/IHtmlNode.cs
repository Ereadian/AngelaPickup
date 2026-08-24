namespace ereadian.builder.html.Content;

using System.Text;

public interface IHtmlNode
{
    NodeType NodeType {get;}
    void Render(in StringBuilder builder, in IDictionary<string, object> variables);
}