namespace ereadian.builder.html.Content;

using System.Text;

public class PlaintNode(string text) : IHtmlNode
{
    public void Render(in StringBuilder builder,  in IDictionary<string, object> variables)
    {
        _ = builder.Append(text);
    }
}