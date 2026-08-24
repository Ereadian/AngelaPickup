namespace ereadian.builder.html.Content;

using System.Text;

public interface IHtmlNode
{
    void Render(in StringBuilder builder, in IDictionary<string, object> variables);
}