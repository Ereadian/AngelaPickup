namespace ereadian.builder.html.Content;

public class NodeCollection
{
    public NodeCollection(HtmlParserContext context)
    {        
        this.Nodes = [];
    }

    public IReadOnlyList<IHtmlNode> Nodes {get;}
}