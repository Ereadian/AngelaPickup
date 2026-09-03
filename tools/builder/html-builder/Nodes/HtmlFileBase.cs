namespace ereadian.builder.html.Nodes;

using System.Text;

public abstract class HtmlFileBase : INodeCollection
{
    public HtmlFileBase(string fullPath, string folder, bool allowComment)
    {
        this.AllowComment = allowComment;

        HtmlParserContext context = new (fullPath, folder, allowComment);
        this.Nodes = Utility.LoadNodes(context);

        Dictionary<string, object> variables = context.Variables;
        this.Variables = variables;
    }

    public Dictionary<string, object> Variables {get;}
    public IReadOnlyList<IHtmlNode> Nodes {get;}

    public NodeType NodeType => NodeType.File;

    protected bool AllowComment {get;}

    public virtual void Render(in StringBuilder builder, in IDictionary<string, object> variables)
    {
        Utility.RenderNodes(this.Nodes, builder, variables);
    }
}