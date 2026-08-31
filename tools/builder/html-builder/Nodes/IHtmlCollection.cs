namespace ereadian.builder.html.Nodes;

public interface INodeCollection : IHtmlNode
{
    IReadOnlyList<IHtmlNode> Nodes {get;}
}