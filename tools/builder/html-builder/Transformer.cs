using System.Globalization;

namespace ereadian.builder.html;

public class Transformer
{
    private readonly CultureInfo siteCultureInfo;

    private readonly Dictionary<string, object> variables = new()
    {
      {VariableNames.SiteBuildTime, DateTime.UtcNow }
    };

    public Transformer(string templateFolder, string siteCultureName)
        : this(templateFolder, CultureInfo.GetCultureInfo(siteCultureName))
    {
    }

    public Transformer(string templateFolder, CultureInfo cultureInfo)
    {
        this.siteCultureInfo = cultureInfo;
    }

    public IDictionary<string, object> Variables => this.variables;
    public CultureInfo SiteCultureInfo => this.siteCultureInfo;

    public string Process(string sourceContent)
    {
        return sourceContent;
    }
}
