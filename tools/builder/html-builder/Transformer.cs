namespace ereadian.builder.html;

using System.Globalization;
using ereadian.builder.html.Content;

public class Transformer
{
    private readonly Dictionary<string, object> globalVariables; 

    public Transformer(string templateFolder, string siteCultureName)
        : this(templateFolder, CultureInfo.GetCultureInfo(siteCultureName))
    {
    }

    public Transformer(string templateFolder, CultureInfo cultureInfo)
    {
        this.SiteCultureInfo = cultureInfo;
        this.globalVariables = new()
        {
            {VariableNames.TemplateFolder, templateFolder },
            {VariableNames.TemplateCollection, new Dictionary<string, HtmlFile>() },
            {VariableNames.SiteCulture, cultureInfo },
            {VariableNames.SiteBuildTime, DateTime.UtcNow },
        };
    }

    public CultureInfo SiteCultureInfo {get;}

    public string Process(string sourceContent)
    {
        return sourceContent;
    }

    public string ProcessFile(string fullPath, string folder)
    {
        return this.Process(File.ReadAllText(fullPath));
    }
}
