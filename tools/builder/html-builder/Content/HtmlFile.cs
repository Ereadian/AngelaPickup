namespace ereadian.builder.html.Content;

using System.Text;

public class HtmlFile : INodeCollection
{
    public HtmlFile(string fullPath, string folder)
    {
        Dictionary<string, object> variables = [];
        HtmlParserContext context = new (fullPath, folder);
        this.Variables = variables;
        this.Nodes = Utility.LoadNodes(context, variables);
        string? templateName = null;
        if (variables.TryGetValue(VariableNames.TemplateName, out object? value))
        {
            templateName = value as string;
        }

        this.TemplateName = templateName is null ? string.Empty : templateName.Trim();
    }

    public string TemplateName {get;}
    public Dictionary<string, object> Variables {get;}
    public IReadOnlyList<IHtmlNode> Nodes {get;}

    public NodeType NodeType => NodeType.File;

    public void Render(in StringBuilder builder, in IDictionary<string, object> variables)
    {
        foreach(IHtmlNode node in this.Nodes)
        {
            node.Render(builder, variables);
        }
    }
}