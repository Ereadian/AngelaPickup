namespace ereadian.builder.html.Nodes;

public class HtmlTemplate : HtmlFileBase
{
    public HtmlTemplate(string fullPath, string folder, bool allowComment, string sharedFolder, Dictionary<string, HtmlFile> sharedContents)
        : base(fullPath, folder, allowComment, sharedFolder, sharedContents)
    {
    }

    public override NodeType NodeType =>  NodeType.Template;
}
