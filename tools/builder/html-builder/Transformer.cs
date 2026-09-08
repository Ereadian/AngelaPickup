namespace ereadian.builder.html;

using System.Globalization;
using ereadian.builder.html.Nodes;

public class Transformer
{
    private readonly Dictionary<string, object> globalVariables; 
    private readonly bool allowComment;

    public Transformer(string outputRootFolder, string templateFolder, string siteCultureName, bool allowComment)
        : this(outputRootFolder, templateFolder, CultureInfo.GetCultureInfo(siteCultureName), allowComment)
    {
    }

    public Transformer(string outputRootFolder, string templateFolder, CultureInfo cultureInfo, bool allowComment)
    {
        this.SiteCultureInfo = cultureInfo;
        this.allowComment = allowComment;
        this.globalVariables = new()
        {
            {VariableNames.OutputRootFolder, outputRootFolder },
            {VariableNames.TemplateFolder, templateFolder },
            {VariableNames.TemplateCollection, new Dictionary<string, HtmlTemplate>() },
            {VariableNames.SiteCulture, cultureInfo },
            {VariableNames.SiteBuildTime, DateTime.UtcNow },
        };
    }

    public CultureInfo SiteCultureInfo {get;}

    public string ProcessFile(string fullPath, string folder)
    {
        HtmlFile file = new HtmlFile(fullPath, folder, this.allowComment);
        return file.Render(this.globalVariables);
    }
}
