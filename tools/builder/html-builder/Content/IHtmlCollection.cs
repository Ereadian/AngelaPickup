namespace ereadian.builder.html.Content;

public interface INodeCollection : IHtmlNode
{
    IReadOnlyList<IHtmlNode> Nodes {get;}
}