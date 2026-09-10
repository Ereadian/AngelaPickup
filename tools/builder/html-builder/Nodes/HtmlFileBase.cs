namespace ereadian.builder.html.Nodes;

using System.Text;

public class HtmlFileBase : INodeCollection
{
    public HtmlFileBase(string fullPath, string folder, bool allowComment, string sharedFolder, Dictionary<string, List<IHtmlNode>> sharedContents)
    {
        this.AllowComment = allowComment;
        this.SharedFolder = sharedFolder;
        this.SharedContents = sharedContents;

        HtmlParserContext context = new (fullPath, folder, allowComment);
        this.Nodes = Utility.LoadNodes(context, sharedFolder, sharedContents);

        Dictionary<string, object> variables = context.Variables;
        this.Variables = variables;
    }

    public Dictionary<string, object> Variables {get;}
    public IReadOnlyList<IHtmlNode> Nodes {get;}

    public virtual NodeType NodeType => NodeType.File;

    public bool AllowComment {get;}

    protected string SharedFolder {get;}
    protected Dictionary<string, List<IHtmlNode>> SharedContents {get;}

    public virtual void Render(in StringBuilder builder, in IDictionary<string, object> variables)
    {
        Utility.RenderNodes(this.Nodes, builder, variables);
    }
}