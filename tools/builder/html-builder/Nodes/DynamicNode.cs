namespace ereadian.builder.html.Nodes;

using System.Text;

public class DynamicNode(
    ElementNode elementNode,
    Action<ElementNode, StringBuilder, IDictionary<string, object>> buildAction) : IHtmlNode
{
    public NodeType NodeType => NodeType.Dynamic;

    public void Render(in StringBuilder builder, in IDictionary<string, object> variables)
    {
        buildAction.Invoke(elementNode, builder, variables);
    }
}