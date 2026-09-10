namespace ereadian.builder.html.Nodes;

using System.Text;

public class HtmlTemplate : HtmlFileBase
{
    public HtmlTemplate(string fullPath, string folder, bool allowComment, string sharedFolder, Dictionary<string, List<IHtmlNode>> sharedContents)
        : base(fullPath, folder, allowComment, sharedFolder, sharedContents)
    {
    }

    public override NodeType NodeType =>  NodeType.Template;
}
